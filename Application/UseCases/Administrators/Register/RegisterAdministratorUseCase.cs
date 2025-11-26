// Application/UseCases/Administrators/Register/RegisterAdministratorUseCase.cs


using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.Mappers;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Application.UseCases.Administrators.Register
{
    /// <summary>
    /// Caso de uso responsável por orquestrar o fluxo de registro de um novo Administrador.
    /// </summary>
    /// <remarks>
    /// Este caso de uso aplica as regras de negócio necessárias para criação de administradores,
    /// incluindo a verificação de duplicidade de e-mail e a conversão do DTO para os Value Objects
    /// e entidades de domínio. A persistência é realizada por meio do repositório de administradores.
    /// </remarks>
    public sealed class RegisterAdministratorUseCase
    {
        private readonly IAdministratorRepository _administratorRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso responsável pelo registro de administradores.
        /// </summary>
        /// <param name="administratorRepository">Repositório utilizado para operações relacionadas à persistência de administradores.</param>
        public RegisterAdministratorUseCase(IAdministratorRepository administratorRepository)
        {
            _administratorRepository = administratorRepository;
        }

        /// <summary>
        /// Executa o fluxo de registro de um novo administrador com base nos dados fornecidos.
        /// </summary>
        /// <param name="request">Objeto contendo os dados necessários para criação do administrador.</param>
        /// <returns>
        /// Um <see cref="RegisterAdministratorResponse"/> contendo o identificador do administrador criado.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando já existe um administrador cadastrado com o e-mail informado.
        /// </exception>
        public async Task<RegisterAdministratorResponse> Handle(RegisterAdministratorRequest request)
        {
            // Converte e valida os value objects
            var email = new Email(request.Email);
            var phone = new Phone(request.Phone);
            var password = Password.Create(request.Password);

            // Verificação de duplicidade de e-mail
            bool emailAlreadyExists = await _administratorRepository.ExistsByEmailAsync(email);

            if (emailAlreadyExists)
            {
                throw new InvalidOperationException(
                    "Já existe um administrador cadastrado com o e-mail informado."
                );
            }

            // Construção da entidade Administrator
            var administrator = new Administrator
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = email,
                Password = password,
                Phone = phone,
                AccessLevel = request.AccessLevel,
                Status = UserStatus.Active
            };

            // Garante que veio ao menos um consent
            if (request.Consents is null || !request.Consents.Any())
            {
                throw new InvalidOperationException("É obrigatório informar ao menos um consentimento.");
            }

            // Associação dos consentimentos fornecidos ao administrador
            foreach (var dto in request.Consents)
            {
                var consent = ConsentMapper.ToDomain(dto);
                administrator.Consents.Add(consent);
            }

            // Usa o método do model para validar consentimento ativo
            var treatmentConsent = administrator.GetActiveConsent(ConsentScope.Treatment);

            if (treatmentConsent is null)
            {
                throw new InvalidOperationException(
                    "O administrador não possui um consentimento de tratamento ativo."
                );
            }


            // Persistência da entidade
            await _administratorRepository.AddAsync(administrator);

            return new RegisterAdministratorResponse(administrator.Id);
        }
    }
}

