// Application/UseCases/InventoryItems/Read/ConsultInventoryItemUseCase.cs

using SGHSS.Application.Interfaces.Repositories;

namespace SGHSS.Application.UseCases.InventoryItems.Consult
{
    /// <summary>
    /// Caso de uso responsável por consultar os dados de um item de estoque
    /// vinculado a uma unidade de saúde específica.
    /// </summary>
    /// <remarks>
    /// Este caso de uso garante que o item retornado pertence à unidade de saúde
    /// informada, permitindo recuperar informações como nome, unidade de medida
    /// e quantidade atual em estoque.
    /// </remarks>
    public class ConsultInventoryItemUseCase
    {
        private readonly IInventoryItemRepository _inventoryItemRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso de consulta de item de estoque.
        /// </summary>
        /// <param name="inventoryItemRepository">
        /// Repositório responsável pela recuperação de itens de estoque.
        /// </param>
        public ConsultInventoryItemUseCase(IInventoryItemRepository inventoryItemRepository)
        {
            _inventoryItemRepository = inventoryItemRepository;
        }

        /// <summary>
        /// Manipula o fluxo de consulta de um item de estoque associado
        /// a uma unidade de saúde.
        /// </summary>
        /// <param name="request">
        /// Dados de entrada contendo o identificador do item e da unidade de saúde.
        /// </param>
        /// <returns>
        /// Um <see cref="ConsultInventoryItemResponse"/> com as informações
        /// consolidadas do item consultado.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Lançada quando o <paramref name="request"/> é nulo.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando os identificadores informados são inválidos ou
        /// quando o item não é encontrado para a unidade de saúde informada.
        /// </exception>
        public async Task<ConsultInventoryItemResponse> Handle(ConsultInventoryItemRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (request.InventoryItemId == Guid.Empty)
                throw new InvalidOperationException("É obrigatório informar um identificador de item de estoque válido.");

            if (request.HealthUnitId == Guid.Empty)
                throw new InvalidOperationException("É obrigatório informar um identificador de unidade de saúde válido.");

            // 1) Carrega o item garantindo vínculo com a unidade de saúde
            var inventoryItem = await _inventoryItemRepository
                .GetByIdAndHealthUnitIdAsync(request.InventoryItemId, request.HealthUnitId);

            if (inventoryItem is null)
                throw new InvalidOperationException("Item de estoque não encontrado para os identificadores informados.");

            // 2) Monta o response
            return new ConsultInventoryItemResponse
            {
                InventoryItemId = inventoryItem.Id,
                Name = inventoryItem.Name,
                Description = inventoryItem.Description,
                UnitOfMeasure = inventoryItem.UnitOfMeasure.Value,
                StockQuantity = inventoryItem.StockQuantity,
                HealthUnitId = inventoryItem.HealthUnit.Id
            };
        }
    }
}
