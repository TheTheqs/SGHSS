// Interface/Controllers/EletronicPrescriptionsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGHSS.Application.UseCases.EletronicPrescriptions.Issue;
using SGHSS.Application.UseCases.EletronicPrescriptions.Read;
using SGHSS.Application.UseCases.LogActivities.Register;
using SGHSS.Domain.Enums;

namespace SGHSS.Interface.Controllers
{
    /// <summary>
    /// Controlador responsável por operações relacionadas a prescrições eletrônicas,
    /// incluindo emissão e futuras consultas/auditorias.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EletronicPrescriptionsController : BaseApiController
    {
        private readonly IssueEletronicPrescriptionUseCase _issueEletronicPrescriptionUseCase;
        private readonly GetPatientEletronicPrescriptionsUseCase _getPatientEletronicPrescriptionsUseCase;

        /// <summary>
        /// Cria uma nova instância do controlador de prescrições eletrônicas.
        /// </summary>
        /// <param name="issueEletronicPrescriptionUseCase">
        /// Caso de uso responsável pela emissão de prescrições eletrônicas.
        /// </param>
        /// <param name="getPatientEletronicPrescriptionsUseCase">
        /// Caso de uso responsável pela consulta de prescrições eletrônicas
        /// associadas a um paciente.
        /// </param>
        /// <param name="registerLogActivityUseCase">
        /// Caso de uso responsável por registrar logs de atividade.
        /// </param>
        public EletronicPrescriptionsController(
            IssueEletronicPrescriptionUseCase issueEletronicPrescriptionUseCase,
            GetPatientEletronicPrescriptionsUseCase getPatientEletronicPrescriptionsUseCase,
            RegisterLogActivityUseCase registerLogActivityUseCase)
            : base(registerLogActivityUseCase)
        {
            _issueEletronicPrescriptionUseCase = issueEletronicPrescriptionUseCase;
            _getPatientEletronicPrescriptionsUseCase = getPatientEletronicPrescriptionsUseCase;
        }

        /// <summary>
        /// Emite uma nova prescrição eletrônica para um paciente.
        /// </summary>
        /// <remarks>
        /// Regra de autorização:
        /// <list type="bullet">
        /// <item>Somente usuários autenticados com nível de acesso
        /// <see cref="AccessLevel.Professional"/> ou superior podem emitir prescrições;</item>
        /// </list>
        ///
        /// Este endpoint delega ao <see cref="IssueEletronicPrescriptionUseCase"/> a lógica de:
        /// <list type="number">
        /// <item>Validar dados de entrada (datas, instruções, assinatura ICP);</item>
        /// <item>Garantir a existência de paciente, profissional, unidade de saúde e consulta;</item>
        /// <item>Mapear a assinatura para o Value Object <c>IcpSignature</c>;</item>
        /// <item>Criar e persistir a entidade <c>EletronicPrescription</c>.</item>
        /// </list>
        /// </remarks>
        /// <param name="request">
        /// Dados necessários para emissão da prescrição eletrônica.
        /// </param>
        /// <returns>
        /// Um <see cref="IssueEletronicPrescriptionResponse"/> contendo os metadados
        /// da prescrição emitida.
        /// </returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(IssueEletronicPrescriptionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IssueEletronicPrescriptionResponse>> Issue(
            [FromBody] IssueEletronicPrescriptionRequest request)
        {
            var accessLevel = GetUserAccessLevel();
            var userId = GetUserId();

            // Apenas profissionais (ou níveis superiores) podem emitir prescrições
            if (accessLevel is null || !HasMinimumAccessLevel(AccessLevel.Professional))
                return Forbid();

            if (request is null)
                return BadRequest(new { error = "O corpo da requisição não pode ser nulo." });

            LogResult logResult = LogResult.Success;
            string logDescription = "Prescrição eletrônica emitida com sucesso.";

            try
            {
                var response = await _issueEletronicPrescriptionUseCase.Handle(request);
                return CreatedAtAction(nameof(Issue), response);
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao emitir prescrição eletrônica: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao emitir prescrição eletrônica.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "EletronicPrescriptions.Issue",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: request.HealthUnitId
                );
            }
        }

        /// <summary>
        /// Retorna todas as prescrições eletrônicas associadas a um paciente específico.
        /// </summary>
        /// <remarks>
        /// Regra de autorização:
        /// <list type="bullet">
        /// <item>Somente usuários autenticados com nível de acesso
        /// <see cref="AccessLevel.Professional"/> ou superior podem consultar
        /// prescrições eletrônicas de pacientes;</item>
        /// </list>
        /// A consulta retorna as prescrições em formato resumido, utilizando
        /// <see cref="SGHSS.Application.UseCases.Common.EletronicPrescriptionDto"/>.
        /// </remarks>
        /// <param name="patientId">
        /// Identificador do paciente cujas prescrições eletrônicas devem ser consultadas.
        /// </param>
        /// <returns>
        /// Um <see cref="GetPatientEletronicPrescriptionsResponse"/> contendo o identificador
        /// do paciente e a lista de prescrições eletrônicas associadas a ele.
        /// </returns>
        [HttpGet("patient/{patientId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(GetPatientEletronicPrescriptionsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<GetPatientEletronicPrescriptionsResponse>> GetPatientEletronicPrescriptions(
            [FromRoute] Guid patientId)
        {
            var accessLevel = GetUserAccessLevel();
            var userId = GetUserId();

            // Sem nível de acesso válido → acesso negado
            if (accessLevel is null || accessLevel.Value < AccessLevel.Patient)
                return Forbid();

            // Se for exatamente Patient, só pode consultar seus próprios atestados.
            if (accessLevel.Value == AccessLevel.Patient)
            {
                if (!userId.HasValue || userId.Value != patientId)
                {
                    return Forbid();
                }
            }

            LogResult logResult = LogResult.Success;
            string logDescription = "Consulta de prescrições eletrônicas do paciente realizada com sucesso.";

            var request = new GetPatientEletronicPrescriptionsRequest
            {
                PatientId = patientId
            };

            try
            {
                var response = await _getPatientEletronicPrescriptionsUseCase.Handle(request);
                return Ok(response);
            }
            catch (ArgumentNullException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao consultar prescrições eletrônicas do paciente: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao consultar prescrições eletrônicas do paciente: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao consultar prescrições eletrônicas do paciente.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "EletronicPrescriptions.GetByPatient",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }
    }
}
