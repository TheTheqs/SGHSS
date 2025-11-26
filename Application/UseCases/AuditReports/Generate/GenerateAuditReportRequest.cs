// Application/UseCases/AuditReports/Generate/GenerateAuditReportRequest.cs

namespace SGHSS.Application.UseCases.AuditReports.Generate
{
    /// <summary>
    /// Representa os dados necessários para a geração de um relatório de auditoria
    /// em um intervalo de tempo específico.
    /// </summary>
    /// <remarks>
    /// Este DTO é utilizado como entrada para o caso de uso responsável por
    /// consolidar eventos de log em um relatório de auditoria, associando-o
    /// a um administrador solicitante.
    /// </remarks>
    public sealed class GenerateAuditReportRequest
    {
        /// <summary>
        /// Identificador do administrador responsável pela geração do relatório.
        /// </summary>
        public Guid AdministratorId { get; init; }

        /// <summary>
        /// Data e hora inicial (inclusiva) do intervalo de logs que serão considerados
        /// na composição do relatório.
        /// </summary>
        public DateTimeOffset From { get; init; }

        /// <summary>
        /// Data e hora final (exclusiva) do intervalo de logs que serão considerados
        /// na composição do relatório.
        /// </summary>
        public DateTimeOffset To { get; init; }
    }
}
