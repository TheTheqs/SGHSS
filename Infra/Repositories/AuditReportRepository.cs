// Infra/Repositories/AuditReportRepository.cs

using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Models;
using SGHSS.Infra.Persistence;

namespace SGHSS.Infra.Repositories
{
    /// <summary>
    /// Repositório responsável pelas operações de persistência relacionadas
    /// aos relatórios de auditoria (<see cref="AuditReport"/>).
    /// </summary>
    public class AuditReportRepository : IAuditReportRepository
    {
        private readonly SGHSSDbContext _context;

        /// <summary>
        /// Cria uma nova instância do repositório de relatórios de auditoria.
        /// </summary>
        /// <param name="context">
        /// Contexto de banco de dados utilizado para acesso e persistência dos dados.
        /// </param>
        public AuditReportRepository(SGHSSDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adiciona um novo relatório de auditoria ao banco de dados.
        /// </summary>
        /// <param name="auditReport">
        /// A entidade <see cref="AuditReport"/> que será persistida.
        /// </param>
        /// <remarks>
        /// Esta operação utiliza o rastreamento de alterações do Entity Framework Core,
        /// permitindo incluir o relatório e suas entidades relacionadas em uma única transação.
        /// </remarks>
        public async Task AddAsync(AuditReport auditReport)
        {
            await _context.AuditReports.AddAsync(auditReport);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Recupera todos os relatórios de auditoria gerados por um administrador específico.
        /// </summary>
        /// <param name="administratorId">
        /// Identificador do administrador responsável pela criação dos relatórios.
        /// </param>
        /// <returns>
        /// Uma coleção somente leitura de <see cref="AuditReport"/> pertencentes
        /// ao administrador informado.
        /// </returns>
        public async Task<IReadOnlyCollection<AuditReport>> GetByAdministratorIdAsync(Guid administratorId)
        {
            var query = _context.AuditReports
                .Include(ar => ar.CreatedBy)
                .Where(ar => ar.CreatedBy.Id == administratorId)
                .OrderByDescending(ar => ar.CreatedAt);

            var result = await query.ToListAsync();
            return result;
        }
    }
}
