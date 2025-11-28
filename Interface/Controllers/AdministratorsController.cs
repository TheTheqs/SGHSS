// Interface/Controllers/AdministratorsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.LogActivities.Register;
using SGHSS.Domain.Enums;

namespace SGHSS.Interface.Controllers
{
    /// <summary>
    /// Controlador responsável por criar novos Administradores no sistema.
    /// Apenas Administradores Super possuem permissão para registrar outros administradores.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AdministratorsController : BaseApiController
    {
        private readonly RegisterAdministratorUseCase _registerAdministratorUseCase;

        /// <summary>
        /// Cria uma nova instância do controlador.
        /// </summary>
        /// <param name="registerAdministratorUseCase">Caso de uso responsável por registrar administradores.</param>
        /// <param name="registerLogActivityUseCase">Caso de uso responsável por registrar logs de atividade.</param>
        public AdministratorsController(
            RegisterAdministratorUseCase registerAdministratorUseCase,
            RegisterLogActivityUseCase registerLogActivityUseCase)
            : base(registerLogActivityUseCase)
        {
            _registerAdministratorUseCase = registerAdministratorUseCase;
        }

        /// <summary>
        /// Registra um novo Administrador no sistema.
        /// Somente Administradores Super podem acessar este endpoint.
        /// </summary>
        /// <param name="request">Dados necessários para criar o Administrador.</param>
        /// <returns>Um <see cref="RegisterAdministratorResponse"/> contendo o ID do Administrador criado.</returns>
        [HttpPost("register")]
        [Authorize]
        [ProducesResponseType(typeof(RegisterAdministratorResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RegisterAdministratorResponse>> Register(
            [FromBody] RegisterAdministratorRequest request)
        {
            // Apenas Administrador Super pode criar novos Administradores
            if (!HasMinimumAccessLevel(AccessLevel.Super))
            {
                return Forbid();
            }

            LogResult logResult = LogResult.Success;
            string logDescription = "Administrador criado com sucesso.";
            Guid? userId = GetUserId();

            try
            {
                var response = await _registerAdministratorUseCase.Handle(request);
                return CreatedAtAction(nameof(Register), response);
            }
            catch (ArgumentException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao registrar administrador: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao registrar administrador: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao registrar administrador.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "Administrators.Register",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }
    }
}
