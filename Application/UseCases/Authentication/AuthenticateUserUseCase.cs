// Application/UseCases/Authentication/AuthenticateUserUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.Interfaces.Services;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Application.UseCases.Authentication
{
    /// <summary>
    /// Caso de uso responsável por orquestrar o fluxo de autenticação de usuários.
    /// </summary>
    /// <remarks>
    /// Este caso de uso localiza o usuário pelo e-mail, valida a senha informada
    /// e, em caso de sucesso, gera um token de acesso contendo as informações
    /// necessárias para autorização nas requisições subsequentes.
    /// </remarks>
    public sealed class AuthenticateUserUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        /// <summary>
        /// Cria uma nova instância do caso de uso de autenticação de usuários.
        /// </summary>
        /// <param name="userRepository">Repositório utilizado para consulta de usuários.</param>
        /// <param name="tokenService">Serviço responsável pela geração de tokens de acesso.</param>
        public AuthenticateUserUseCase(
            IUserRepository userRepository,
            ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Executa o fluxo de autenticação com base nas credenciais fornecidas.
        /// </summary>
        /// <param name="request">Objeto contendo e-mail e senha do usuário.</param>
        /// <returns>
        /// Um <see cref="AuthenticateUserResponse"/> contendo o token gerado
        /// e as informações principais do usuário autenticado.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando as credenciais são inválidas ou o usuário não está ativo.
        /// </exception>
        public async Task<AuthenticateUserResponse> Handle(AuthenticateUserRequest request)
        {
            // Converte o e-mail para o Value Object
            var email = new Email(request.Email);

            // Localiza o usuário pelo e-mail
            var user = await _userRepository.GetByEmailAsync(email);

            if (user is null)
            {
                throw new InvalidOperationException("Credenciais inválidas.");
            }

            // Verifica se o usuário está ativo
            if (user.Status != UserStatus.Active)
            {
                throw new InvalidOperationException("O usuário não está ativo no sistema.");
            }

            // Validação da senha usando o VO Password (BCrypt + hash)
            var isPasswordValid = user.Password.Verify(request.Password);

            if (!isPasswordValid)
            {
                throw new InvalidOperationException("Credenciais inválidas.");
            }

            // Determina o tipo concreto e o nível de acesso efetivo
            var userType = user.GetType().Name;

            AccessLevel effectiveAccessLevel = user switch
            {
                Administrator admin => admin.AccessLevel,
                Patient => AccessLevel.Patient,
                Professional => AccessLevel.Professional,
                _ => throw new InvalidOperationException("Tipo de usuário desconhecido para autenticação.")
            };

            // Geração do token de acesso
            var (token, expiresAt) = _tokenService.GenerateToken(user, effectiveAccessLevel);

            // Monta o response
            return new AuthenticateUserResponse
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email.Value,
                UserType = userType,
                AccessLevel = effectiveAccessLevel,
                Token = token,
                ExpiresAt = expiresAt
            };
        }
    }
}
