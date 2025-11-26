// Application/UseCases/AuditReports/Generate/GenerateAuditReportResponse.cs

namespace SGHSS.Application.UseCases.AuditReports.Generate
{
    /// <summary>
    /// Representa o resultado da geração de um relatório de auditoria,
    /// incluindo os metadados principais e o conteúdo consolidado.
    /// </summary>
    public sealed class GenerateAuditReportResponse
    {
        /// <summary>
        /// Identificador único do relatório de auditoria gerado e persistido.
        /// </summary>
        public Guid AuditReportId { get; init; }

        /// <summary>
        /// Identificador do administrador responsável pela criação do relatório.
        /// </summary>
        public Guid AdministratorId { get; init; }

        /// <summary>
        /// Data e hora em que o relatório foi efetivamente gerado e registrado.
        /// </summary>
        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>
        /// Data e hora inicial (inclusiva) do intervalo de logs considerados
        /// na composição do relatório.
        /// </summary>
        public DateTimeOffset From { get; init; }

        /// <summary>
        /// Data e hora final (exclusiva) do intervalo de logs considerados
        /// na composição do relatório.
        /// </summary>
        public DateTimeOffset To { get; init; }

        /// <summary>
        /// Conteúdo textual consolidado do relatório de auditoria, normalmente
        /// composto pela representação formatada das entradas de log do período.
        /// </summary>
        public string ReportDetails { get; init; } = string.Empty;
    }
}
