// Application/UseCases/AuditReports/Consult/ConsultAuditReportsByAdministratorResponse.cs

namespace SGHSS.Application.UseCases.AuditReports.Consult
{
    /// <summary>
    /// Representa o resultado da consulta de relatórios de auditoria
    /// gerados por um administrador específico.
    /// </summary>
    public sealed class ConsultAuditReportsByAdministratorResponse
    {
        /// <summary>
        /// Identificador do administrador ao qual os relatórios pertencem.
        /// </summary>
        public Guid AdministratorId { get; init; }

        /// <summary>
        /// Coleção de relatórios de auditoria associados ao administrador.
        /// </summary>
        /// <remarks>
        /// Cada item contém um resumo dos metadados principais do relatório,
        /// permitindo exibição em listagens e telas de histórico.
        /// </remarks>
        public IReadOnlyCollection<AuditReportSummaryDto> Reports { get; init; }
            = Array.Empty<AuditReportSummaryDto>();
    }

    /// <summary>
    /// Representa um resumo de um relatório de auditoria, contendo
    /// as principais informações para listagem.
    /// </summary>
    public sealed class AuditReportSummaryDto
    {
        /// <summary>
        /// Identificador único do relatório de auditoria.
        /// </summary>
        public Guid AuditReportId { get; init; }

        /// <summary>
        /// Data e hora em que o relatório foi gerado.
        /// </summary>
        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>
        /// Opcionalmente, um trecho ou descrição curta do conteúdo
        /// do relatório, para facilitar a identificação em listagens.
        /// </summary>
        public string Preview { get; init; } = string.Empty;
    }
}
