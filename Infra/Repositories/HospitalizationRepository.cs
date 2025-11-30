// Infra/Repositories/HospitalizationRepository.cs

using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Models;
using SGHSS.Infra.Persistence;

namespace SGHSS.Infra.Repositories
{
    /// <summary>
    /// Repositório responsável pelas operações de persistência relacionadas às
    /// internações (<see cref="Hospitalization"/>).
    /// </summary>
    public class HospitalizationRepository : IHospitalizationRepository
    {
        private readonly SGHSSDbContext _context;

        /// <summary>
        /// Cria uma nova instância do repositório de internações.
        /// </summary>
        /// <param name="context">O contexto de banco de dados utilizado para acesso aos dados.</param>
        public HospitalizationRepository(SGHSSDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Recupera uma internação pelo seu identificador único.
        /// </summary>
        /// <param name="hospitalizationId">O identificador da internação.</param>
        /// <returns>
        /// A entidade <see cref="Hospitalization"/> correspondente ao identificador informado,
        /// ou <c>null</c> caso nenhuma internação seja encontrada.
        /// </returns>
        /// <remarks>
        /// Esta operação deve ser utilizada quando for necessário consultar dados de uma
        /// internação específica, seja para validação, auditoria, geração de relatórios
        /// ou para execução de regras de negócio na camada de aplicação.
        /// </remarks>
        public async Task<Hospitalization?> GetByIdAsync(Guid hospitalizationId)
        {
            return await _context.Hospitalizations
                .Include(h => h.Bed)
                .Include(h => h.Patient)
                .FirstOrDefaultAsync(h => h.Id == hospitalizationId);
        }

        /// <summary>
        /// Adiciona uma nova internação ao repositório.
        /// </summary>
        public async Task AddAsync(Hospitalization hospitalization)
        {
            await _context.Hospitalizations.AddAsync(hospitalization);
            await _context.SaveChangesAsync();
        }
    }
}
