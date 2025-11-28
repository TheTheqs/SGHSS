// Interface/Controllers/HealthUnitsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGHSS.Application.UseCases.Administrators.Read;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.Administrators.Update;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.LogActivities.Register;
using SGHSS.Domain.Enums;

namespace SGHSS.Interface.Controllers
{
    /// <summary>
    /// Controlador responsável por operações relacionadas a unidades de saúde,
    /// incluindo registro e gerenciamento de estrutura (como leitos).
    /// Apenas Administradores com nível Basic (2) ou superior podem acessar estes endpoints.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthUnitsController : BaseApiController
    {
        private readonly RegisterHealthUnitUseCase _registerHealthUnitUseCase;
        private readonly ManageBedsUseCase _manageBedsUseCase;
        private readonly GetAllHealthUnitsUseCase _getAllHealthUnitsUseCase;

        /// <summary>
        /// Instancia um novo controlador de unidades de saúde.
        /// </summary>
        /// <param name="registerHealthUnitUseCase">Caso de uso de registro de unidade.</param>
        /// <param name="manageBedsUseCase">Caso de uso de gerenciamento de leitos da unidade.</param>
        /// <param name="getAllHealthUnitsUseCase">Caso de uso de listagem simplificada de unidades de saúde.</param>
        /// <param name="registerLogActivityUseCase">Caso de uso de registro de log.</param>
        public HealthUnitsController(
            RegisterHealthUnitUseCase registerHealthUnitUseCase,
            ManageBedsUseCase manageBedsUseCase,
            GetAllHealthUnitsUseCase getAllHealthUnitsUseCase,
            RegisterLogActivityUseCase registerLogActivityUseCase)
            : base(registerLogActivityUseCase)
        {
            _registerHealthUnitUseCase = registerHealthUnitUseCase;
            _manageBedsUseCase = manageBedsUseCase;
            _getAllHealthUnitsUseCase = getAllHealthUnitsUseCase;
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

        /// <summary>
        /// Gerencia leitos de uma unidade de saúde existente, permitindo adicionar
        /// ou remover leitos conforme a configuração informada.
        /// Requer um Administrador autenticado com nível Basic ou superior.
        /// </summary>
        /// <param name="request">
        /// Dados da operação de gerenciamento de leitos, incluindo unidade,
        /// modelo de leito, quantidade e tipo de operação (adição/remoção).
        /// </param>
        /// <returns>
        /// Um <see cref="ManageBedsResponse"/> contendo o estado atualizado dos leitos
        /// da unidade após a operação.
        /// </returns>
        [HttpPost("manage-beds")]
        [Authorize]
        [ProducesResponseType(typeof(ManageBedsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ManageBedsResponse>> ManageBeds(
            [FromBody] ManageBedsRequest request)
        {
            if (!HasMinimumAccessLevel(AccessLevel.Basic))
            {
                return Forbid();
            }

            LogResult logResult = LogResult.Success;
            string logDescription = "Leitos da unidade modificados com sucesso.";
            Guid? userId = GetUserId();

            try
            {
                var response = await _manageBedsUseCase.Handle(request);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao gerenciar leitos da unidade: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao gerenciar leitos da unidade: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao gerenciar leitos da unidade.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "HealthUnits.ManageBeds",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: request.HealthUnitId
                );
            }
        }

        /// <summary>
        /// Retorna uma lista simplificada com todas as unidades de saúde registradas no sistema.
        /// Requer um Administrador autenticado com nível Basic ou superior.
        /// </summary>
        /// <returns>
        /// Um <see cref="GetAllResponse"/> contendo os identificadores e nomes das unidades de saúde.
        /// </returns>
        [HttpGet("all")]
        [Authorize]
        [ProducesResponseType(typeof(GetAllResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<GetAllResponse>> GetAll()
        {
            if (!HasMinimumAccessLevel(AccessLevel.Basic))
            {
                return Forbid();
            }

            LogResult logResult = LogResult.Success;
            string logDescription = "Consulta de unidades de saúde realizada com sucesso.";
            Guid? userId = GetUserId();

            try
            {
                var response = await _getAllHealthUnitsUseCase.Handle();
                return Ok(response);
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao consultar unidades de saúde.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "HealthUnits.GetAll",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }
    }
}
