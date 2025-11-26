// Application/UseCases/AuditReports/Consult/ConsultAuditReportsByAdministratorRequest.cs

namespace SGHSS.Application.UseCases.AuditReports.Consult
{
    /// <summary>
    /// Representa os dados necessários para consultar todos os relatórios
    /// de auditoria gerados por um administrador específico.
    /// </summary>
    /// <remarks>
    /// Este DTO é utilizado como entrada para o caso de uso responsável
    /// por recuperar o histórico de relatórios de auditoria associados
    /// a um administrador do sistema.
    /// </remarks>
    public sealed class ConsultAuditReportsByAdministratorRequest
    {
        /// <summary>
        /// Identificador do administrador cujos relatórios de auditoria
        /// serão consultados.
        /// </summary>
        public Guid AdministratorId { get; init; }
    }
}
