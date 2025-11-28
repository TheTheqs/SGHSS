// Interface/Controllers/AuditReportsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGHSS.Application.UseCases.AuditReports.Consult;
using SGHSS.Application.UseCases.AuditReports.Generate;
using SGHSS.Application.UseCases.LogActivities.Register;
using SGHSS.Domain.Enums;

namespace SGHSS.Interface.Controllers
{
    /// <summary>
    /// Controlador responsável por expor endpoints relacionados à geração
    /// e consulta de relatórios de auditoria do sistema.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuditReportsController : BaseApiController
    {
        private readonly GenerateAuditReportUseCase _generateAuditReportUseCase;
        private readonly ConsultAuditReportsByAdministratorUseCase _consultAuditReportsByAdministratorUseCase;

        /// <summary>
        /// Cria uma nova instância do controlador de relatórios de auditoria.
        /// </summary>
        /// <param name="generateAuditReportUseCase">
        /// Caso de uso responsável por gerar e persistir relatórios de auditoria.
        /// </param>
        /// <param name="consultAuditReportsByAdministratorUseCase">
        /// Caso de uso responsável por consultar relatórios de auditoria
        /// gerados por um administrador específico.
        /// </param>
        /// <param name="registerLogActivityUseCase">
        /// Caso de uso responsável por registrar atividades de log.
        /// </param>
        public AuditReportsController(
            GenerateAuditReportUseCase generateAuditReportUseCase,
            ConsultAuditReportsByAdministratorUseCase consultAuditReportsByAdministratorUseCase,
            RegisterLogActivityUseCase registerLogActivityUseCase)
            : base(registerLogActivityUseCase)
        {
            _generateAuditReportUseCase = generateAuditReportUseCase;
            _consultAuditReportsByAdministratorUseCase = consultAuditReportsByAdministratorUseCase;
        }

        /// <summary>
        /// Gera um novo relatório de auditoria para um intervalo de tempo específico.
        /// </summary>
        /// <param name="request">
        /// Objeto contendo o identificador do administrador e o intervalo de datas
        /// a ser considerado na composição do relatório.
        /// </param>
        /// <returns>
        /// Um <see cref="GenerateAuditReportResponse"/> com os metadados e o conteúdo
        /// consolidado do relatório gerado.
        /// </returns>
        [HttpPost("generate")]
        [ProducesResponseType(typeof(GenerateAuditReportResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize]
        public async Task<ActionResult<GenerateAuditReportResponse>> Generate(
            [FromBody] GenerateAuditReportRequest request)
        {
            if (!HasMinimumAccessLevel(AccessLevel.Basic))
            {
                return Forbid();
            }

            LogResult logResult = LogResult.Success;
            string logDescription = "Relatório de auditoria gerado com sucesso.";
            Guid? userId = request.AdministratorId;

            try
            {
                var response = await _generateAuditReportUseCase.Handle(request);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                // Erros de validação de entrada
                logResult = LogResult.Failure;
                logDescription = $"Falha ao gerar relatório de auditoria: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                // Erros de regra de negócio (admin não encontrado, ausência de logs, etc.)
                logResult = LogResult.Failure;
                logDescription = $"Falha ao gerar relatório de auditoria: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (Exception)
            {
                // Erros inesperados sobem para o GlobalExceptionHandler
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao gerar relatório de auditoria.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "AuditReports.Generate",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null);
            }
        }

        /// <summary>
        /// Consulta todos os relatórios de auditoria gerados por um administrador específico.
        /// </summary>
        /// <param name="administratorId">
        /// Identificador do administrador cujos relatórios serão recuperados.
        /// </param>
        /// <returns>
        /// Um <see cref="ConsultAuditReportsByAdministratorResponse"/> contendo
        /// a lista de relatórios associados ao administrador informado.
        /// </returns>
        [HttpGet("administrator/{administratorId:guid}")]
        [ProducesResponseType(typeof(ConsultAuditReportsByAdministratorResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize]
        public async Task<ActionResult<ConsultAuditReportsByAdministratorResponse>> GetByAdministrator(
            Guid administratorId)
        {
            // Apenas ADM Super pode consultar esse endpoint
            if (!HasMinimumAccessLevel(AccessLevel.Super))
            {
                return Forbid();
            }
            LogResult logResult = LogResult.Success;
            string logDescription = "Consulta de relatórios de auditoria realizada com sucesso.";
            Guid? userId = administratorId;

            try
            {
                var request = new ConsultAuditReportsByAdministratorRequest
                {
                    AdministratorId = administratorId
                };

                var response = await _consultAuditReportsByAdministratorUseCase.Handle(request);

                // Aqui você pode decidir se 200 com lista vazia é ok (geralmente é)
                // ou se quer tratar como 404. Vou manter 200 com lista (mesmo vazia).
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                // Por exemplo: administrador não encontrado
                logResult = LogResult.Failure;
                logDescription = $"Falha ao consultar relatórios de auditoria: {ex.Message}";

                return NotFound(new
                {
                    error = ex.Message
                });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao consultar relatórios de auditoria.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "AuditReports.ConsultByAdministrator",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null);
            }
        }
    }
}
