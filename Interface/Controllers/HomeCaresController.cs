// Interface/Controllers/HomeCaresController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGHSS.Application.UseCases.HomeCares.Read;
using SGHSS.Application.UseCases.HomeCares.Register;
using SGHSS.Application.UseCases.LogActivities.Register;
using SGHSS.Domain.Enums;

namespace SGHSS.Interface.Controllers
{
    /// <summary>
    /// Controlador responsável por operações relacionadas a atendimentos
    /// de home care, incluindo registro e futuras consultas/listagens.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HomeCaresController : BaseApiController
    {
        private readonly RegisterHomeCareUseCase _registerHomeCareUseCase;
        private readonly GetPatientHomeCaresUseCase _getPatientHomeCaresUseCase;

        /// <summary>
        /// Cria uma nova instância do controlador de atendimentos de home care.
        /// </summary>
        /// <param name="registerHomeCareUseCase">
        /// Caso de uso responsável pelo registro de atendimentos de home care.
        /// </param>
        /// <param name="registerLogActivityUseCase">
        /// Caso de uso responsável por registrar logs de atividade.
        /// </param>
        public HomeCaresController(
            RegisterHomeCareUseCase registerHomeCareUseCase,
            RegisterLogActivityUseCase registerLogActivityUseCase,
            GetPatientHomeCaresUseCase getPatientHomeCaresUseCase)
            : base(registerLogActivityUseCase)
        {
            _registerHomeCareUseCase = registerHomeCareUseCase;
            _getPatientHomeCaresUseCase = getPatientHomeCaresUseCase;
        }

        /// <summary>
        /// Registra um novo atendimento de home care para um paciente.
        /// </summary>
        /// <remarks>
        /// Regra de autorização:
        /// <list type="bullet">
        /// <item>Somente usuários autenticados com nível de acesso
        /// <see cref="AccessLevel.Professional"/> ou superior podem registrar
        /// atendimentos de home care;</item>
        /// </list>
        /// O caso de uso valida a existência do paciente, do profissional e da
        /// unidade de saúde antes de persistir o registro.
        /// </remarks>
        /// <param name="request">
        /// Dados necessários para criação do registro de home care.
        /// </param>
        /// <returns>
        /// Um <see cref="RegisterHomeCareResponse"/> contendo o identificador
        /// do atendimento recém-registrado.
        /// </returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(RegisterHomeCareResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RegisterHomeCareResponse>> Register(
            [FromBody] RegisterHomeCareRequest request)
        {
            var accessLevel = GetUserAccessLevel();
            var userId = GetUserId();

            // Somente Professional ou superior pode registrar home care
            if (accessLevel is null || !HasMinimumAccessLevel(AccessLevel.Professional))
                return Forbid();

            LogResult logResult = LogResult.Success;
            string logDescription = "Atendimento de home care registrado com sucesso.";

            try
            {
                var response = await _registerHomeCareUseCase.Handle(request);

                // Created com location simbólica, seguindo padrão de outros endpoints
                return CreatedAtAction(nameof(Register), new { id = response.HomeCareId }, response);
            }
            catch (ArgumentNullException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao registrar atendimento de home care: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao registrar atendimento de home care: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao registrar atendimento de home care.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "HomeCares.Register",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: request.HealthUnitId
                );
            }
        }

        /// <summary>
        /// Retorna os atendimentos de home care associados a um paciente específico.
        /// </summary>
        /// <remarks>
        /// Regra de autorização:
        /// <list type="bullet">
        /// <item>Somente usuários autenticados com nível de acesso
        /// <see cref="AccessLevel.Professional"/> ou superior podem consultar
        /// os registros de home care de um paciente;</item>
        /// </list>
        /// </remarks>
        /// <param name="patientId">
        /// Identificador único do paciente cujos registros de home care
        /// devem ser consultados.
        /// </param>
        /// <returns>
        /// Um <see cref="GetPatientHomeCaresResponse"/> contendo a lista de
        /// registros de home care do paciente.
        /// </returns>
        [HttpGet("patient/{patientId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(GetPatientHomeCaresResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<GetPatientHomeCaresResponse>> GetPatientHomeCares(
            [FromRoute] Guid patientId)
        {
            var accessLevel = GetUserAccessLevel();
            var userId = GetUserId();

            // Somente Professional ou superior pode consultar home care
            if (accessLevel is null || !HasMinimumAccessLevel(AccessLevel.Professional))
                return Forbid();

            LogResult logResult = LogResult.Success;
            string logDescription = "Consulta de atendimentos de home care do paciente realizada com sucesso.";

            var request = new GetPatientHomeCaresRequest
            {
                PatientId = patientId
            };

            try
            {
                var response = await _getPatientHomeCaresUseCase.Handle(request);
                return Ok(response);
            }
            catch (ArgumentNullException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao consultar atendimentos de home care do paciente: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao consultar atendimentos de home care do paciente: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao consultar atendimentos de home care do paciente.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "HomeCares.GetByPatient",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }
    }
}
