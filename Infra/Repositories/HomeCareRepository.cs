// Infra/Repositories/HomeCareRepository.cs

using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Models;
using SGHSS.Infra.Persistence;

namespace SGHSS.Infra.Repositories
{
    /// <summary>
    /// Repositório responsável por realizar operações de persistência
    /// relacionadas à entidade <see cref="HomeCare"/> utilizando o
    /// Entity Framework Core.
    /// </summary>
    /// <remarks>
    /// Esta implementação segue o padrão de repositório e fornece uma
    /// abstração sobre o acesso ao banco, permitindo que a camada de
    /// aplicação trabalhe sem dependências diretas do EF Core.
    /// </remarks>
    public class HomeCareRepository : IHomeCareRepository
    {
        private readonly SGHSSDbContext _context;

        /// <summary>
        /// Cria uma nova instância do repositório de registros de home care.
        /// </summary>
        /// <param name="context">
        /// O contexto de banco de dados utilizado para acesso aos dados.
        /// </param>
        public HomeCareRepository(SGHSSDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Persiste um novo registro de home care no banco de dados.
        /// </summary>
        /// <param name="homeCare">Instância de <see cref="HomeCare"/> a ser salva.</param>
        public async Task AddAsync(HomeCare homeCare)
        {
            await _context.HomeCares.AddAsync(homeCare);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Atualiza um registro de home care existente no banco de dados.
        /// </summary>
        /// <param name="homeCare">
        /// Entidade <see cref="HomeCare"/> com os dados já modificados e pronta
        /// para ser persistida.
        /// </param>
        public async Task UpdateAsync(HomeCare homeCare)
        {
            _context.HomeCares.Update(homeCare);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Recupera um registro de home care pelo seu identificador único.
        /// </summary>
        /// <param name="homeCareId">Identificador do registro de home care a ser localizado.</param>
        /// <returns>
        /// A entidade <see cref="HomeCare"/> correspondente ao ID informado,
        /// ou <c>null</c> caso nenhum registro seja encontrado.
        /// </returns>
        /// <remarks>
        /// Inclui o carregamento do paciente, do profissional e da unidade
        /// de saúde associados ao registro de home care.
        /// </remarks>
        public async Task<HomeCare?> GetByIdAsync(Guid homeCareId)
        {
            return await _context.HomeCares
                .Include(hc => hc.Patient)
                .Include(hc => hc.Professional)
                .Include(hc => hc.HealthUnit)
                .FirstOrDefaultAsync(hc => hc.Id == homeCareId);
        }

        /// <summary>
        /// Recupera todos os registros de home care associados a um paciente.
        /// </summary>
        public async Task<IReadOnlyCollection<HomeCare>> GetByPatientIdAsync(Guid patientId)
        {
            return await _context.HomeCares
                .Include(hc => hc.Patient)
                .Include(hc => hc.Professional)
                .Include(hc => hc.HealthUnit)
                .Where(hc => hc.Patient.Id == patientId)
                .OrderByDescending(hc => hc.Date)
                .ToListAsync();
        }
    }
}
