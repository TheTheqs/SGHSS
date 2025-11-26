// Application/Interfaces/Repositories/IAuditReportRepository.cs

using SGHSS.Domain.Models;

namespace SGHSS.Application.Interfaces.Repositories
{
    /// <summary>
    /// Define o contrato para operações de persistência relacionadas à entidade
    /// <see cref="AuditReport"/>, permitindo o registro e a consulta de relatórios
    /// de auditoria gerados por administradores do sistema.
    /// </summary>
    public interface IAuditReportRepository
    {
        /// <summary>
        /// Adiciona um novo relatório de auditoria ao repositório.
        /// </summary>
        /// <param name="auditReport">
        /// Instância de <see cref="AuditReport"/> a ser persistida.
        /// </param>
        /// <remarks>
        /// Este método deve ser utilizado sempre que um relatório de auditoria
        /// for gerado por um administrador e precisar ser armazenado para
        /// fins de rastreabilidade, conformidade ou consultas futuras.
        /// </remarks>
        Task AddAsync(AuditReport auditReport);

        /// <summary>
        /// Recupera todos os relatórios de auditoria gerados por um administrador específico.
        /// </summary>
        /// <param name="administratorId">
        /// Identificador do administrador responsável pela criação dos relatórios.
        /// </param>
        /// <returns>
        /// Uma coleção somente leitura contendo todas as instâncias de
        /// <see cref="AuditReport"/> associadas ao administrador informado.
        /// </returns>
        /// <remarks>
        /// A implementação deve ordenar os resultados em ordem decrescente de
        /// <see cref="AuditReport.CreatedAt"/>, de forma a apresentar primeiro
        /// os relatórios mais recentes.
        /// </remarks>
        Task<IReadOnlyCollection<AuditReport>> GetByAdministratorIdAsync(Guid administratorId);
    }
}
