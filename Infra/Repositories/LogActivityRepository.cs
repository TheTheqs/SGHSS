// Infra/Repositories/LogActivityRepository.cs

using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Models;
using SGHSS.Infra.Persistence;

namespace SGHSS.Infra.Repositories
{
    /// <summary>
    /// Repositório responsável pelas operações de persistência relacionadas
    /// aos registros de atividade (<see cref="LogActivity"/>).
    /// </summary>
    public class LogActivityRepository : ILogActivityRepository
    {
        private readonly SGHSSDbContext _context;

        /// <summary>
        /// Cria uma nova instância do repositório de logs de atividade.
        /// </summary>
        /// <param name="context">
        /// Contexto de banco de dados utilizado para acesso e persistência dos dados.
        /// </param>
        public LogActivityRepository(SGHSSDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adiciona um novo registro de atividade ao banco de dados.
        /// </summary>
        /// <param name="logActivity">
        /// A entidade <see cref="LogActivity"/> que será persistida.
        /// </param>
        /// <remarks>
        /// Esta operação utiliza o rastreamento de alterações do Entity Framework Core,
        /// permitindo incluir o log e suas entidades relacionadas em uma única transação.
        /// </remarks>
        public async Task AddAsync(LogActivity logActivity)
        {
            await _context.LogActivities.AddAsync(logActivity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Recupera todos os registros de atividade dentro do intervalo de tempo informado.
        /// </summary>
        /// <param name="from">
        /// Data e hora inicial (inclusiva) do intervalo de consulta.
        /// </param>
        /// <param name="to">
        /// Data e hora final (exclusiva) do intervalo de consulta.
        /// </param>
        /// <returns>
        /// Uma coleção somente leitura de <see cref="LogActivity"/> pertencentes
        /// ao intervalo especificado.
        /// </returns>
        public async Task<IReadOnlyCollection<LogActivity>> GetByPeriodAsync(
            DateTimeOffset from,
            DateTimeOffset to)
        {
            var query = _context.LogActivities
                .Include(l => l.User)
                .Include(l => l.HealthUnit)
                .Where(l => l.Timestamp >= from && l.Timestamp < to)
                .OrderBy(l => l.Timestamp);

            var result = await query.ToListAsync();
            return result;
        }
    }
}
