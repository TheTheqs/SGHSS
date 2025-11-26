// Application/UseCases/AuditReports/Generate/GenerateAuditReportUseCase.cs

using System.Text;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Models;

namespace SGHSS.Application.UseCases.AuditReports.Generate
{
    /// <summary>
    /// Caso de uso responsável por gerar e persistir um relatório de auditoria
    /// com base nas atividades de log registradas em um intervalo de tempo.
    /// </summary>
    /// <remarks>
    /// Este caso de uso consolida as entradas de <see cref="LogActivity"/> em um
    /// único relatório textual, associando-o a um administrador responsável e
    /// registrando-o na base de dados para futuras consultas.
    /// </remarks>
    public class GenerateAuditReportUseCase
    {
        private readonly IAuditReportRepository _auditReportRepository;
        private readonly ILogActivityRepository _logActivityRepository;
        private readonly IAdministratorRepository _administratorRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso de geração de relatórios de auditoria.
        /// </summary>
        /// <param name="auditReportRepository">
        /// Repositório responsável pela persistência de relatórios de auditoria.
        /// </param>
        /// <param name="logActivityRepository">
        /// Repositório responsável pelo acesso aos registros de atividades de log.
        /// </param>
        /// <param name="administratorRepository">
        /// Repositório responsável pelo acesso aos administradores do sistema.
        /// </param>
        public GenerateAuditReportUseCase(
            IAuditReportRepository auditReportRepository,
            ILogActivityRepository logActivityRepository,
            IAdministratorRepository administratorRepository)
        {
            _auditReportRepository = auditReportRepository;
            _logActivityRepository = logActivityRepository;
            _administratorRepository = administratorRepository;
        }

        /// <summary>
        /// Executa o fluxo de geração de um relatório de auditoria para o intervalo informado.
        /// </summary>
        /// <param name="request">
        /// Objeto contendo o identificador do administrador solicitante e o intervalo
        /// de tempo que será utilizado para filtrar os registros de log.
        /// </param>
        /// <returns>
        /// Um <see cref="GenerateAuditReportResponse"/> contendo os metadados do relatório
        /// gerado e o conteúdo textual consolidado.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Lançada quando o request é nulo.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Lançada quando o identificador de administrador é vazio ou quando o intervalo
        /// de datas é inválido (por exemplo, <c>From</c> maior ou igual a <c>To</c>).
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando o administrador informado não é encontrado.
        /// </exception>
        public async Task<GenerateAuditReportResponse> Handle(GenerateAuditReportRequest request)
        {
            // Validação defensiva básica
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request), "A requisição não pode ser nula.");
            }

            if (request.AdministratorId == Guid.Empty)
            {
                throw new ArgumentException(
                    "O identificador do administrador não pode ser vazio.",
                    nameof(request.AdministratorId));
            }

            if (request.From >= request.To)
            {
                throw new ArgumentException("O intervalo de datas informado é inválido. A data inicial deve ser menor que a final.");
            }

            // Verifica se o administrador existe
            var administrator = await _administratorRepository.GetByIdAsync(request.AdministratorId);

            if (administrator is null)
            {
                throw new InvalidOperationException(
                    "Não foi possível localizar um administrador para o identificador informado.");
            }

            // Busca os logs no período
            var logs = await _logActivityRepository.GetByPeriodAsync(request.From, request.To);

            // Consolida o conteúdo do relatório a partir dos ToString() de cada LogActivity
            var now = DateTimeOffset.UtcNow;

            var builder = new StringBuilder();

            builder.AppendLine("====== AUDIT REPORT ======");
            builder.AppendLine($"GeneratedAt:   {now:yyyy-MM-dd HH:mm:ss zzz}");
            builder.AppendLine($"AdminId:       {administrator.Id}");
            builder.AppendLine($"AdminName:     {administrator.Name}");
            builder.AppendLine($"Period.From:   {request.From:yyyy-MM-dd HH:mm:ss zzz}");
            builder.AppendLine($"Period.To:     {request.To:yyyy-MM-dd HH:mm:ss zzz}");
            builder.AppendLine($"TotalEntries:  {logs.Count}");
            builder.AppendLine("==========================");
            builder.AppendLine();

            if (logs.Count == 0)
            {
                builder.AppendLine("Nenhum registro de atividade foi encontrado para o período informado.");
            }
            else
            {
                foreach (var log in logs)
                {
                    builder.AppendLine(log.ToString());
                    builder.AppendLine(); // linha em branco entre blocos
                }
            }

            string reportDetails = builder.ToString();

            // Monta a entidade de relatório
            var auditReport = new AuditReport
            {
                CreatedAt = now,
                ReportDetails = reportDetails,
                CreatedBy = administrator
            };

            // Persiste o relatório
            await _auditReportRepository.AddAsync(auditReport);

            // Retorna o response com os dados principais e o conteúdo
            return new GenerateAuditReportResponse
            {
                AuditReportId = auditReport.Id,
                AdministratorId = administrator.Id,
                CreatedAt = auditReport.CreatedAt,
                From = request.From,
                To = request.To,
                ReportDetails = reportDetails
            };
        }
    }
}
