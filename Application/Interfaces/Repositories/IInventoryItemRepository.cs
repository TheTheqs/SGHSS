// Application/Interfaces/Repositories/IInventoryItemRepository.cs

using SGHSS.Domain.Models;

namespace SGHSS.Application.Interfaces.Repositories
{
    /// <summary>
    /// Define as operações de persistência relacionadas aos itens de estoque
    /// no sistema. Esta interface abstrai o acesso ao repositório responsável
    /// por criar e recuperar itens de inventário no banco de dados.
    /// </summary>
    /// <remarks>
    /// Implementações dessa interface devem garantir que as operações sejam
    /// realizadas de maneira consistente e reflitam corretamente o estado
    /// dos itens de estoque associados às unidades de saúde.
    /// </remarks>
    public interface IInventoryItemRepository
    {
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
        /// A responsabilidade de validar regras de negócio e consistência
        /// dos dados antes da inserção pertence à camada de aplicação.
        /// </remarks>
        Task AddAsync(InventoryItem inventoryItem);

        /// <summary>
        /// Recupera um item de estoque pelo seu identificador único.
        /// </summary>
        /// <param name="inventoryItemId">O identificador do item de estoque.</param>
        /// <returns>
        /// A entidade <see cref="InventoryItem"/> correspondente ao identificador informado,
        /// ou <c>null</c> caso não seja encontrada.
        /// </returns>
        /// <remarks>
        /// Este método pode ser utilizado para recuperar o estado atual de um item de estoque
        /// após sua criação ou antes de operações posteriores, como ajustes de quantidade
        /// ou auditoria de movimentações.
        /// </remarks>
        Task<InventoryItem?> GetByIdAsync(Guid inventoryItemId);

        /// <summary>
        /// Recupera um item de estoque a partir de seu identificador e da unidade de saúde
        /// à qual ele deve estar associado.
        /// </summary>
        /// <param name="inventoryItemId">O identificador do item de estoque.</param>
        /// <param name="healthUnitId">O identificador da unidade de saúde.</param>
        /// <returns>
        /// A entidade <see cref="InventoryItem"/> correspondente aos identificadores informados,
        /// ou <c>null</c> caso não seja encontrada ou não pertença à unidade de saúde informada.
        /// </returns>
        /// <remarks>
        /// Este método é especialmente útil em cenários onde é necessário garantir
        /// a integridade do vínculo entre o item de estoque e a unidade de saúde,
        /// como em consultas e operações de ajuste de quantidade.
        /// </remarks>
        Task<InventoryItem?> GetByIdAndHealthUnitIdAsync(Guid inventoryItemId, Guid healthUnitId);

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
        /// Este método deve sobrescrever os valores atuais do item no banco de dados
        /// com os novos dados fornecidos. A responsabilidade de validar regras de 
        /// negócio antes da atualização é da camada de aplicação.
        /// </remarks>
        Task UpdateAsync(InventoryItem inventoryItem);
    }
}
