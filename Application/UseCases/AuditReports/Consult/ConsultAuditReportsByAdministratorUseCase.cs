// Application/UseCases/AuditReports/Consult/ConsultAuditReportsByAdministratorUseCase.cs

using SGHSS.Application.Interfaces.Repositories;

namespace SGHSS.Application.UseCases.AuditReports.Consult
{
    /// <summary>
    /// Caso de uso responsável por consultar todos os relatórios de auditoria
    /// gerados por um administrador específico.
    /// </summary>
    /// <remarks>
    /// Este caso de uso encapsula a lógica de validação do administrador
    /// informado e de projeção dos relatórios de auditoria em DTOs de resumo,
    /// adequados para exibição em listagens e telas de histórico.
    /// </remarks>
    public class ConsultAuditReportsByAdministratorUseCase
    {
        private readonly IAuditReportRepository _auditReportRepository;
        private readonly IAdministratorRepository _administratorRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso de consulta de relatórios
        /// de auditoria por administrador.
        /// </summary>
        /// <param name="auditReportRepository">
        /// Repositório responsável pelo acesso aos relatórios de auditoria.
        /// </param>
        /// <param name="administratorRepository">
        /// Repositório responsável pelo acesso aos administradores do sistema.
        /// </param>
        public ConsultAuditReportsByAdministratorUseCase(
            IAuditReportRepository auditReportRepository,
            IAdministratorRepository administratorRepository)
        {
            _auditReportRepository = auditReportRepository;
            _administratorRepository = administratorRepository;
        }

        /// <summary>
        /// Executa o fluxo de consulta de relatórios de auditoria associados
        /// ao administrador informado.
        /// </summary>
        /// <param name="request">
        /// Objeto contendo o identificador do administrador cujos relatórios
        /// serão consultados.
        /// </param>
        /// <returns>
        /// Um <see cref="ConsultAuditReportsByAdministratorResponse"/> contendo
        /// o identificador do administrador e a coleção de relatórios de auditoria
        /// por ele gerados.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Lançada quando o request é nulo.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Lançada quando o identificador do administrador é vazio.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando o administrador informado não é encontrado.
        /// </exception>
        public async Task<ConsultAuditReportsByAdministratorResponse> Handle(
            ConsultAuditReportsByAdministratorRequest request)
        {
            // Validação defensiva básica
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request), "A requisição não pode ser nula.");
            }

            if (request.AdministratorId == Guid.Empty)
            {
                throw new ArgumentException("O identificador do administrador não pode ser vazio.");
            }

            // Verifica se o administrador existe
            var administrator = await _administratorRepository.GetByIdAsync(request.AdministratorId);

            if (administrator is null)
            {
                throw new InvalidOperationException(
                    "Não foi possível localizar um administrador para o identificador informado.");
            }

            // Recupera os relatórios do repositório
            var auditReports = await _auditReportRepository.GetByAdministratorIdAsync(request.AdministratorId);

            // Projeta para DTOs de resumo
            var reportSummaries = auditReports
                .Select(report =>
                {
                    // Gera um pequeno preview do conteúdo do relatório
                    var details = report.ReportDetails ?? string.Empty;

                    // Pode ser a primeira linha ou primeiros N caracteres
                    var firstLine = details.Split(Environment.NewLine)
                        .FirstOrDefault(line => !string.IsNullOrWhiteSpace(line))
                        ?? string.Empty;

                    string preview;

                    if (firstLine.Length > 200)
                    {
                        preview = firstLine.Substring(0, 200) + "...";
                    }
                    else
                    {
                        preview = firstLine;
                    }

                    return new AuditReportSummaryDto
                    {
                        AuditReportId = report.Id,
                        CreatedAt = report.CreatedAt,
                        Preview = preview
                    };
                })
                .ToList()
                .AsReadOnly();

            // Monta o response final
            return new ConsultAuditReportsByAdministratorResponse
            {
                AdministratorId = administrator.Id,
                Reports = reportSummaries
            };
        }
    }
}
