// Infra/Repositories/HealthUnitRepository.cs

using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;
using SGHSS.Infra.Persistence;

namespace SGHSS.Infra.Repositories
{
    /// <summary>
    /// Repositório responsável pelas operações de persistência relacionadas a unidades de saúde.
    /// </summary>
    public class HealthUnitRepository : IHealthUnitRepository
    {
        private readonly SGHSSDbContext _context;

        /// <summary>
        /// Cria uma nova instância do repositório de unidades de saúde.
        /// </summary>
        /// <param name="context">O contexto de banco de dados utilizado para acesso aos dados.</param>
        public HealthUnitRepository(SGHSSDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Determina de forma assíncrona se existe uma unidade de saúde cadastrada com o CNPJ especificado.
        /// </summary>
        /// <param name="cnpj">CNPJ encapsulado em um Value Object.</param>
        /// <returns>
        /// <c>true</c> se existir uma unidade com o CNPJ informado; caso contrário, <c>false</c>.
        /// </returns>
        public async Task<bool> ExistsByCnpjAsync(Cnpj cnpj)
        {
            return await _context.HealthUnits
                .AnyAsync(h => h.Cnpj == cnpj);
        }

        /// <summary>
        /// Adiciona uma nova unidade de saúde à base de dados, incluindo seus relacionamentos configurados.
        /// </summary>
        /// <param name="healthUnit">A entidade <see cref="HealthUnit"/> que será persistida.</param>
        /// <remarks>
        /// Esta operação utiliza o rastreamento de alterações do Entity Framework Core,
        /// permitindo incluir a unidade e suas entidades agregadas em uma única transação.
        /// </remarks>
        public async Task AddAsync(HealthUnit healthUnit)
        {
            await _context.HealthUnits.AddAsync(healthUnit);
            await _context.SaveChangesAsync();
        }
        /// <summary>
        /// Recupera uma unidade de saúde pelo seu identificador único,
        /// incluindo sua lista de leitos e demais agregados relevantes.
        /// </summary>
        /// <param name="healthUnitId">Identificador da unidade de saúde.</param>
        /// <returns>
        /// A entidade <see cref="HealthUnit"/> correspondente ao identificador,
        /// ou <c>null</c> caso não seja encontrada.
        /// </returns>
        /// <remarks>
        /// Inclui o carregamento explícito dos leitos, uma vez que eles fazem parte do agregado
        /// e são frequentemente necessários em operações de gerenciamento.
        /// </remarks>
        public async Task<HealthUnit?> GetByIdAsync(Guid healthUnitId)
        {
            return await _context.HealthUnits
                .Include(h => h.Beds)
                .FirstOrDefaultAsync(h => h.Id == healthUnitId);
        }

        /// <summary>
        /// Atualiza uma unidade de saúde existente na base de dados,
        /// persistindo alterações em seus agregados (como lista de leitos).
        /// </summary>
        /// <param name="healthUnit">
        /// Instância atualizada da entidade que será persistida.
        /// </param>
        /// <remarks>
        /// Esta operação utiliza o rastreamento de alterações do Entity Framework Core.
        /// Caso a instância já esteja sendo rastreada, o contexto detectará as modificações
        /// automaticamente.
        /// </remarks>
        public async Task UpdateAsync(HealthUnit healthUnit)
        {
            _context.HealthUnits.Update(healthUnit);
            await _context.SaveChangesAsync();
        }
        /// <summary>
        /// Recupera todas as unidades de saúde cadastradas na base de dados.
        /// </summary>
        /// <returns>
        /// Uma lista somente leitura contendo todas as instâncias de <see cref="HealthUnit"/>.
        /// </returns>
        public async Task<IReadOnlyList<HealthUnit>> GetAllAsync()
        {
            var health_units = _context.HealthUnits
                               .Include(h => h.Beds);

            health_units
                .SelectMany(hu => hu.Beds)
                .ToList()
                .ForEach(bed => bed.Status = Domain.Enums.BedStatus.Available);

            return await _context.HealthUnits
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
