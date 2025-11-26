// Infra/Repositories/InventoryItemRepository.cs

using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Models;
using SGHSS.Infra.Persistence;

namespace SGHSS.Infra.Repositories
{
    /// <summary>
    /// Repositório responsável pelas operações de persistência relacionadas
    /// aos itens de estoque (<see cref="InventoryItem"/>).
    /// </summary>
    public class InventoryItemRepository : IInventoryItemRepository
    {
        private readonly SGHSSDbContext _context;

        /// <summary>
        /// Cria uma nova instância do repositório de itens de estoque.
        /// </summary>
        /// <param name="context">O contexto de banco de dados utilizado para acesso aos dados.</param>
        public InventoryItemRepository(SGHSSDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Persiste um novo item de estoque no repositório.
        /// </summary>
        /// <param name="inventoryItem">
        /// A entidade <see cref="InventoryItem"/> que será adicionada ao banco de dados.
        /// </param>
        /// <returns>
        /// Uma tarefa assíncrona que representa a operação de criação.
        /// </returns>
        /// <remarks>
        /// Este método apenas inclui o registro no contexto e salva as alterações.
        /// Validações de negócio são responsabilidade da camada de aplicação.
        /// </remarks>
        public async Task AddAsync(InventoryItem inventoryItem)
        {
            await _context.InventoryItems.AddAsync(inventoryItem);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Recupera um item de estoque pelo seu identificador único.
        /// </summary>
        /// <param name="inventoryItemId">O identificador do item de estoque.</param>
        /// <returns>
        /// A entidade <see cref="InventoryItem"/> correspondente ao identificador informado,
        /// ou <c>null</c> caso não seja encontrada.
        /// </returns>
        /// <remarks>
        /// Este método pode ser usado após o cadastro ou em operações que precisem
        /// inspecionar o estado atual do item, incluindo sua quantidade em estoque
        /// e vínculos com a unidade de saúde.
        /// </remarks>
        public async Task<InventoryItem?> GetByIdAsync(Guid inventoryItemId)
        {
            return await _context.InventoryItems
                .Include(i => i.HealthUnit)
                .Include(i => i.InventoryMovement)
                .FirstOrDefaultAsync(i => i.Id == inventoryItemId);
        }

        /// <summary>
        /// Recupera um item de estoque a partir de seu identificador e da unidade de saúde
        /// à qual ele está associado.
        /// </summary>
        /// <param name="inventoryItemId">O identificador do item de estoque.</param>
        /// <param name="healthUnitId">O identificador da unidade de saúde.</param>
        /// <returns>
        /// A entidade <see cref="InventoryItem"/> correspondente aos identificadores informados,
        /// ou <c>null</c> caso não seja encontrada ou não pertença à unidade de saúde informada.
        /// </returns>
        public async Task<InventoryItem?> GetByIdAndHealthUnitIdAsync(Guid inventoryItemId, Guid healthUnitId)
        {
            return await _context.InventoryItems
                .Include(i => i.HealthUnit)
                .Include(i => i.InventoryMovement)
                .FirstOrDefaultAsync(i =>
                    i.Id == inventoryItemId &&
                    i.HealthUnit.Id == healthUnitId);
        }

        /// <summary>
        /// Atualiza os dados de um item de estoque existente no repositório.
        /// </summary>
        /// <param name="inventoryItem">
        /// A entidade <see cref="InventoryItem"/> contendo os dados atualizados.
        /// </param>
        /// <returns>
        /// Uma tarefa assíncrona que representa a operação de atualização.
        /// </returns>
        /// <remarks>
        /// Se a entidade estiver desacoplada do contexto, ela será anexada e marcada
        /// como modificada antes da persistência. Regras de negócio devem ser
        /// validadas previamente pela camada de aplicação.
        /// </remarks>
        public async Task UpdateAsync(InventoryItem inventoryItem)
        {
            var entry = _context.Entry(inventoryItem);

            if (entry.State == EntityState.Detached)
            {
                _context.InventoryItems.Attach(inventoryItem);
                entry.State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
        }
    }
}
