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
        private readonly ConsultHealthUnitBedsUseCase _consultHealthUnitBedsUseCase;
        private readonly MakeBedAsUnderMaintenanceUseCase _makeBedAsUnderMaintenanceUseCase;
        private readonly MakeBedAsAvailableUseCase _makeBedAsAvailableUseCase;

        /// <summary>
        /// Instancia um novo controlador de unidades de saúde.
        /// </summary>
        /// <param name="registerHealthUnitUseCase">Caso de uso de registro de unidade.</param>
        /// <param name="manageBedsUseCase">Caso de uso de gerenciamento de leitos da unidade.</param>
        /// <param name="getAllHealthUnitsUseCase">Caso de uso de listagem simplificada de unidades de saúde.</param>
        /// <param name="consultHealthUnitBedsUseCase">Caso de uso de consulta de leitos de uma unidade.</param>
        /// <param name="registerLogActivityUseCase">Caso de uso de registro de log.</param>
        /// <param name="makeBedAsAvailableUseCase">Caso de uso para marcar uma cama como livre novamente.</param>
        /// <param name="makeBedAsUnderMaintenanceUseCase">Caso de uso que coloca uma cama em manutenção.</param>
        public HealthUnitsController(
            RegisterHealthUnitUseCase registerHealthUnitUseCase,
            ManageBedsUseCase manageBedsUseCase,
            GetAllHealthUnitsUseCase getAllHealthUnitsUseCase,
            ConsultHealthUnitBedsUseCase consultHealthUnitBedsUseCase,
            MakeBedAsUnderMaintenanceUseCase makeBedAsUnderMaintenanceUseCase,
            MakeBedAsAvailableUseCase makeBedAsAvailableUseCase,
            RegisterLogActivityUseCase registerLogActivityUseCase
        ) : base(registerLogActivityUseCase)
        {
            _registerHealthUnitUseCase = registerHealthUnitUseCase;
            _manageBedsUseCase = manageBedsUseCase;
            _getAllHealthUnitsUseCase = getAllHealthUnitsUseCase;
            _consultHealthUnitBedsUseCase = consultHealthUnitBedsUseCase;
            _makeBedAsUnderMaintenanceUseCase = makeBedAsUnderMaintenanceUseCase;
            _makeBedAsAvailableUseCase = makeBedAsAvailableUseCase;
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
        /// Consulta os leitos de uma unidade de saúde, permitindo filtragem opcional
        /// por tipo e status de leito.
        /// Requer um Administrador autenticado com nível Basic ou superior.
        /// </summary>
        /// <param name="request">
        /// Dados da consulta, incluindo o identificador da unidade de saúde e filtros opcionais.
        /// </param>
        /// <returns>
        /// Um <see cref="ConsultHealthUnitBedsResponse"/> contendo a lista de leitos
        /// da unidade após aplicação dos filtros.
        /// </returns>
        [HttpGet("beds")]
        [Authorize]
        [ProducesResponseType(typeof(ConsultHealthUnitBedsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ConsultHealthUnitBedsResponse>> ConsultBeds(
            [FromQuery] ConsultHealthUnitBedsRequest request)
        {
            if (!HasMinimumAccessLevel(AccessLevel.Basic))
            {
                return Forbid();
            }

            LogResult logResult = LogResult.Success;
            string logDescription = "Consulta de leitos da unidade realizada com sucesso.";
            Guid? userId = GetUserId();

            try
            {
                var response = await _consultHealthUnitBedsUseCase.Handle(request);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao consultar leitos da unidade: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao consultar leitos da unidade: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao consultar leitos da unidade.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "HealthUnits.ConsultBeds",
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

        /// <summary>
        /// Define uma cama como "em manutenção", tornando-a temporariamente indisponível
        /// para internações, reservas ou qualquer operação clínica.
        /// </summary>
        /// <remarks>
        /// Apenas Administradores com nível de acesso <see cref="AccessLevel.Basic"/> ou superior
        /// podem executar esta operação.
        ///
        /// A cama só pode ser colocada em manutenção quando estiver no estado
        /// <see cref="BedStatus.Available"/>. Caso esteja ocupada, reservada ou já em manutenção,
        /// uma exceção é lançada.
        /// </remarks>
        /// <param name="bedId">Identificador único da cama a ser modificada.</param>
        /// <returns>Retorna <c>NoContent</c> quando a operação ocorre com sucesso.</returns>
        [HttpPatch("beds/{bedId:guid}/maintenance")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetBedUnderMaintenance(Guid bedId)
        {
            if (!HasMinimumAccessLevel(AccessLevel.Basic))
                return Forbid();

            LogResult logResult = LogResult.Success;
            string logDescription = "Cama marcada como em manutenção.";
            Guid? userId = GetUserId();

            try
            {
                await _makeBedAsUnderMaintenanceUseCase.Handle(bedId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                logResult = LogResult.Failure;
                logDescription = ex.Message;
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = ex.Message;
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao colocar cama em manutenção.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "HealthUnits.SetBedUnderMaintenance",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }

        /// <summary>
        /// Marca uma cama como "disponível" novamente, removendo o estado de manutenção
        /// e permitindo que ela volte ao uso normal na unidade.
        /// </summary>
        /// <remarks>
        /// Apenas Administradores com nível de acesso <see cref="AccessLevel.Basic"/> ou superior
        /// podem executar esta operação.
        ///
        /// A cama somente pode voltar a ficar disponível caso esteja no estado
        /// <see cref="BedStatus.UnderMaintenance"/>. Se estiver em qualquer outro estado,
        /// uma exceção é lançada para impedir inconsistências.
        /// </remarks>
        /// <param name="bedId">Identificador único da cama que será alterada.</param>
        /// <returns>Retorna <c>NoContent</c> quando a operação ocorre com sucesso.</returns>
        [HttpPatch("beds/{bedId:guid}/available")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetBedAvailable(Guid bedId)
        {
            if (!HasMinimumAccessLevel(AccessLevel.Basic))
                return Forbid();

            LogResult logResult = LogResult.Success;
            string logDescription = "Cama marcada como disponível.";
            Guid? userId = GetUserId();

            try
            {
                await _makeBedAsAvailableUseCase.Handle(bedId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                logResult = LogResult.Failure;
                logDescription = ex.Message;
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = ex.Message;
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao marcar cama como disponível.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "HealthUnits.SetBedAvailable",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }
    }
}
