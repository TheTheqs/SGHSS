// Interface/Controllers/EletronicPrescriptionsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGHSS.Application.UseCases.EletronicPrescriptions.Issue;
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

        /// <summary>
        /// Cria uma nova instância do controlador de prescrições eletrônicas.
        /// </summary>
        /// <param name="issueEletronicPrescriptionUseCase">
        /// Caso de uso responsável pela emissão de prescrições eletrônicas.
        /// </param>
        /// <param name="registerLogActivityUseCase">
        /// Caso de uso responsável por registrar logs de atividade.
        /// </param>
        public EletronicPrescriptionsController(
            IssueEletronicPrescriptionUseCase issueEletronicPrescriptionUseCase,
            RegisterLogActivityUseCase registerLogActivityUseCase)
            : base(registerLogActivityUseCase)
        {
            _issueEletronicPrescriptionUseCase = issueEletronicPrescriptionUseCase;
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
    }
}
