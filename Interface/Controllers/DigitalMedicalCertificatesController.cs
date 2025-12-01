// Interface/Controllers/DigitalMedicalCertificatesController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGHSS.Application.UseCases.DigitalMedicalCertificates.Issue;
using SGHSS.Application.UseCases.LogActivities.Register;
using SGHSS.Domain.Enums;

namespace SGHSS.Interface.Controllers
{
    /// <summary>
    /// Controlador responsável por operações relacionadas a atestados médicos digitais,
    /// incluindo a emissão de novos atestados para pacientes.
    /// </summary>
    /// <remarks>
    /// Todas as operações deste controlador exigem autenticação e são restritas
    /// a usuários com nível de acesso exatamente <see cref="AccessLevel.Professional"/>.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class DigitalMedicalCertificatesController : BaseApiController
    {
        private readonly IssueDigitalMedicalCertificateUseCase _issueDigitalMedicalCertificateUseCase;

        /// <summary>
        /// Cria uma nova instância do controlador de atestados médicos digitais.
        /// </summary>
        public DigitalMedicalCertificatesController(
            IssueDigitalMedicalCertificateUseCase issueDigitalMedicalCertificateUseCase,
            RegisterLogActivityUseCase registerLogActivityUseCase)
            : base(registerLogActivityUseCase)
        {
            _issueDigitalMedicalCertificateUseCase = issueDigitalMedicalCertificateUseCase;
        }

        /// <summary>
        /// Emite um novo atestado médico digital para um paciente, associando-o
        /// a uma consulta, unidade de saúde e profissional específicos.
        /// </summary>
        /// <remarks>
        /// Este endpoint:
        /// <list type="bullet">
        /// <item>Valida os dados de entrada do atestado (datas, recomendações, assinatura ICP);</item>
        /// <item>Garante a existência de paciente, profissional, unidade de saúde e consulta;</item>
        /// <item>Cria e persiste um <c>DigitalMedicalCertificate</c> no prontuário digital do paciente.</item>
        /// </list>
        /// Apenas usuários autenticados com nível de acesso exatamente
        /// <see cref="AccessLevel.Professional"/> podem emitir atestados.
        /// </remarks>
        /// <param name="request">Dados necessários para a emissão do atestado médico digital.</param>
        /// <returns>
        /// Um <see cref="IssueDigitalMedicalCertificateResponse"/> contendo os metadados
        /// do atestado emitido.
        /// </returns>
        [HttpPost("issue")]
        [Authorize]
        [ProducesResponseType(typeof(IssueDigitalMedicalCertificateResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IssueDigitalMedicalCertificateResponse>> IssueDigitalMedicalCertificate(
            [FromBody] IssueDigitalMedicalCertificateRequest request)
        {
            // Apenas usuários com nível EXATAMENTE Professional
            if (!HasExactAccessLevel(AccessLevel.Professional))
                return Forbid();

            LogResult logResult = LogResult.Success;
            string logDescription = "Atestado médico digital emitido com sucesso.";
            Guid? userId = GetUserId();

            try
            {
                var response = await _issueDigitalMedicalCertificateUseCase.Handle(request);

                // 201 Created, indicando criação de recurso
                return CreatedAtAction(nameof(IssueDigitalMedicalCertificate), response);
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao emitir atestado médico digital: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao emitir atestado médico digital.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "DigitalMedicalCertificates.Issue",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: request.HealthUnitId
                );
            }
        }
    }
}
