// Interface/Controllers/AuthController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGHSS.Application.UseCases.Authentication;
using SGHSS.Application.UseCases.LogActivities.Register;
using SGHSS.Domain.Enums;

namespace SGHSS.Interface.Controllers
{
    /// <summary>
    /// Controlador responsável por expor o endpoint de autenticação de usuários,
    /// permitindo a obtenção de um token JWT a partir de credenciais válidas.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseApiController
    {
        private readonly AuthenticateUserUseCase _authenticateUserUseCase;

        /// <summary>
        /// Cria uma nova instância do controlador de autenticação.
        /// </summary>
        /// <param name="authenticateUserUseCase">
        /// Caso de uso responsável por autenticar usuários e gerar o token de acesso.
        /// </param>
        /// <param name="registerLogActivityUseCase">
        /// Caso de uso responsável por registrar atividades de log.
        /// </param>
        public AuthController(
            AuthenticateUserUseCase authenticateUserUseCase,
            RegisterLogActivityUseCase registerLogActivityUseCase)
            : base(registerLogActivityUseCase)
        {
            _authenticateUserUseCase = authenticateUserUseCase;
        }

        /// <summary>
        /// Autentica um usuário com base nas credenciais fornecidas e retorna
        /// um token JWT para acesso às demais rotas protegidas da API.
        /// </summary>
        /// <param name="request">
        /// Objeto contendo e-mail e senha em texto puro.
        /// </param>
        /// <returns>
        /// Um <see cref="AuthenticateUserResponse"/> contendo o token gerado
        /// e as informações principais do usuário autenticado, caso as credenciais
        /// sejam válidas.
        /// </returns>
        /// <remarks>
        /// Este endpoint é público (não requer autenticação prévia),
        /// pois é justamente o responsável por gerar o token de acesso.
        /// </remarks>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthenticateUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthenticateUserResponse>> Login(
            [FromBody] AuthenticateUserRequest request)
        {
            // Valores usados para o log
            LogResult logResult = LogResult.Success;
            string logDescription = "Login realizado com sucesso.";
            Guid? userId = null;

            try
            {
                var response = await _authenticateUserUseCase.Handle(request);

                userId = response.UserId;

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                // Falhas de autenticação esperadas (credenciais inválidas, usuário inativo, etc.)
                logResult = LogResult.Failure;
                logDescription = $"Falha na autenticação: {ex.Message}";

                return Unauthorized(new
                {
                    error = ex.Message
                });
            }
            catch (Exception)
            {
                // Qualquer erro inesperado também será marcado como falha no log
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao autenticar o usuário.";

                // Deixa o GlobalExceptionHandlingMiddleware tratar
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId: userId,
                    action: logResult == LogResult.Success ? "Auth.Login" : "Auth.LoginFailed",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }
    }
}
