// Application/UseCases/InventoryItems/Register/RegisterInventoryItemUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Application.UseCases.InventoryItems.Register
{
    /// <summary>
    /// Caso de uso responsável por registrar um novo item de estoque
    /// vinculado a uma unidade de saúde.
    /// </summary>
    /// <remarks>
    /// Este caso de uso recebe os dados necessários para criação de um
    /// <see cref="InventoryItem"/>, valida a unidade de saúde associada
    /// e persiste o item no repositório de inventário.
    /// </remarks>
    public class RegisterInventoryItemUseCase
    {
        private readonly IInventoryItemRepository _inventoryItemRepository;
        private readonly IHealthUnitRepository _healthUnitRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso de registro de item de estoque.
        /// </summary>
        /// <param name="inventoryItemRepository">
        /// Repositório responsável pelas operações de persistência de itens de estoque.
        /// </param>
        /// <param name="healthUnitRepository">
        /// Repositório responsável pela recuperação das unidades de saúde.
        /// </param>
        public RegisterInventoryItemUseCase(
            IInventoryItemRepository inventoryItemRepository,
            IHealthUnitRepository healthUnitRepository)
        {
            _inventoryItemRepository = inventoryItemRepository;
            _healthUnitRepository = healthUnitRepository;
        }

        /// <summary>
        /// Manipula o fluxo de cadastro de um novo item de estoque.
        /// </summary>
        /// <param name="request">
        /// Dados de entrada necessários para criação do item de inventário.
        /// </param>
        /// <returns>
        /// Um <see cref="RegisterInventoryItemResponse"/> contendo os dados
        /// consolidados do item recém-cadastrado.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Lançada quando o <paramref name="request"/> é nulo.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando a unidade de saúde informada não é encontrada
        /// ou quando os dados de entrada violam regras básicas de negócio.
        /// </exception>
        public async Task<RegisterInventoryItemResponse> Handle(RegisterInventoryItemRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (request.HealthUnitId == Guid.Empty)
                throw new InvalidOperationException("É obrigatório informar uma unidade de saúde válida para o item de estoque.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new InvalidOperationException("É obrigatório informar o nome do item de estoque.");

            if (string.IsNullOrWhiteSpace(request.UnitOfMeasure))
                throw new InvalidOperationException("É obrigatório informar a unidade de medida do item de estoque.");

            if (request.StockQuantity < 0)
                throw new InvalidOperationException("A quantidade em estoque não pode ser negativa.");

            // 1) Carrega a unidade de saúde
            var healthUnit = await _healthUnitRepository.GetByIdAsync(request.HealthUnitId);
            if (healthUnit is null)
                throw new InvalidOperationException("Unidade de saúde informada não foi encontrada.");

            // 2) Converte a unidade de medida para o Value Object
            var unitOfMeasure = new UnitOfMeasure(request.UnitOfMeasure);

            // 3) Cria a entidade de domínio
            var inventoryItem = new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Description = request.Description?.Trim() ?? string.Empty,
                UnitOfMeasure = unitOfMeasure,
                StockQuantity = request.StockQuantity,
                HealthUnit = healthUnit,
            };

            // 4) Persiste o item
            await _inventoryItemRepository.AddAsync(inventoryItem);

            // 5) Monta o response
            return new RegisterInventoryItemResponse
            {
                Id = inventoryItem.Id,
                Name = inventoryItem.Name,
                Description = inventoryItem.Description,
                UnitOfMeasure = inventoryItem.UnitOfMeasure.Value,
                StockQuantity = inventoryItem.StockQuantity,
                HealthUnitId = healthUnit.Id
            };
        }
    }
}
