// Interface/Controllers/HealthUnitsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.LogActivities.Register;
using SGHSS.Domain.Enums;

namespace SGHSS.Interface.Controllers
{
    /// <summary>
    /// Controlador responsável por registrar novas unidades de saúde no sistema.
    /// Apenas Administradores com nível Basic (2) ou superior podem acessar este endpoint.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthUnitsController : BaseApiController
    {
        private readonly RegisterHealthUnitUseCase _registerHealthUnitUseCase;
        /// <summary>
        /// Instancia um novo controlador de unidades de saúde.
        /// </summary>
        /// <param name="registerHealthUnitUseCase">Caso de uso de registro de unidade.</param>
        /// <param name="registerLogActivityUseCase">Caso de uso de registro de log.</param>
        public HealthUnitsController(
            RegisterHealthUnitUseCase registerHealthUnitUseCase,
            RegisterLogActivityUseCase registerLogActivityUseCase)
            : base(registerLogActivityUseCase)
        {
            _registerHealthUnitUseCase = registerHealthUnitUseCase;
        }

        /// <summary>
        /// Registra uma nova unidade de saúde no sistema.
        /// Requer um Administrador autenticado com nível Basic ou superior.
        /// </summary>
        /// <param name="request">Dados necessários para criar a unidade.</param>
        /// <returns>ID da unidade recém-criada.</returns>
        [HttpPost("register")]
        [Authorize]
        [ProducesResponseType(typeof(RegisterHealthUnitResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RegisterHealthUnitResponse>> Register(
            [FromBody] RegisterHealthUnitRequest request)
        {
            // Apenas Administradores Basic (2) ou superior
            if (!HasMinimumAccessLevel(AccessLevel.Basic))
            {
                return Forbid();
            }

            LogResult logResult = LogResult.Success;
            string logDescription = "Unidade de saúde criada com sucesso.";
            Guid? userId = GetUserId();

            try
            {
                var response = await _registerHealthUnitUseCase.Handle(request);
                return CreatedAtAction(nameof(Register), response);
            }
            catch (ArgumentException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao registrar unidade de saúde: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao registrar unidade de saúde: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao registrar unidade de saúde.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "HealthUnits.Register",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }
    }
}
