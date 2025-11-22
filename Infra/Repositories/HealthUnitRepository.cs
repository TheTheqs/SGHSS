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
    }
}
