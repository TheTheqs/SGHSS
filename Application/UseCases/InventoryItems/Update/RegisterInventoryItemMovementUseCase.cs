// Application/UseCases/InventoryItems/Update/RegisterInventoryMovementUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;

namespace SGHSS.Application.UseCases.InventoryItems.Update
{
    /// <summary>
    /// Caso de uso responsável por registrar um movimento de estoque
    /// (entrada, saída ou ajuste) para um item de inventário, atualizando
    /// a quantidade disponível e mantendo o histórico de movimentações.
    /// </summary>
    /// <remarks>
    /// Este caso de uso garante:
    /// <list type="bullet">
    /// <item>Validação dos identificadores informados.</item>
    /// <item>Existência do item e vínculo com a unidade de saúde.</item>
    /// <item>Existência do administrador responsável.</item>
    /// <item>Consistência do estoque (sem valores negativos).</item>
    /// </list>
    /// Após a movimentação, o estado atualizado do item é retornado.
    /// </remarks>
    public class RegisterInventoryMovementUseCase
    {
        private readonly IInventoryItemRepository _inventoryItemRepository;
        private readonly IAdministratorRepository _administratorRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso de registro de movimentação de estoque.
        /// </summary>
        /// <param name="inventoryItemRepository">
        /// Repositório responsável pela recuperação e atualização de itens de estoque.
        /// </param>
        /// <param name="administratorRepository">
        /// Repositório responsável pela recuperação de administradores.
        /// </param>
        public RegisterInventoryMovementUseCase(
            IInventoryItemRepository inventoryItemRepository,
            IAdministratorRepository administratorRepository)
        {
            _inventoryItemRepository = inventoryItemRepository;
            _administratorRepository = administratorRepository;
        }

        /// <summary>
        /// Manipula o fluxo de registro de uma nova movimentação de estoque,
        /// atualizando a quantidade do item e registrando o histórico.
        /// </summary>
        /// <param name="request">
        /// Dados de entrada contendo informações do item, unidade de saúde,
        /// quantidade, tipo de movimento e administrador responsável.
        /// </param>
        /// <returns>
        /// Um <see cref="RegisterInventoryMovementResponse"/> representando
        /// o estado atualizado do item de estoque após a movimentação.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Lançada quando o <paramref name="request"/> é nulo.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando algum identificador é inválido, o item não é encontrado
        /// para a unidade informada, o administrador não existe ou a operação
        /// viola regras de consistência de estoque.
        /// </exception>
        public async Task<RegisterInventoryMovementResponse> Handle(RegisterInventoryMovementRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (request.InventoryItemId == Guid.Empty)
                throw new InvalidOperationException("É obrigatório informar um identificador de item de estoque válido.");

            if (request.HealthUnitId == Guid.Empty)
                throw new InvalidOperationException("É obrigatório informar um identificador de unidade de saúde válido.");

            if (request.AdministratorId == Guid.Empty)
                throw new InvalidOperationException("É obrigatório informar um identificador de administrador válido.");

            if (request.Quantity <= 0)
                throw new InvalidOperationException("A quantidade da movimentação deve ser maior que zero.");

            // 1) Carrega o item garantindo vínculo com a unidade de saúde
            InventoryItem? inventoryItem = await _inventoryItemRepository
                .GetByIdAndHealthUnitIdAsync(request.InventoryItemId, request.HealthUnitId);

            if (inventoryItem is null)
                throw new InvalidOperationException("Item de estoque não encontrado para os identificadores informados.");

            // 2) Carrega o administrador responsável
            var administrator = await _administratorRepository.GetByIdAsync(request.AdministratorId);
            if (administrator is null)
                throw new InvalidOperationException("Administrador não encontrado para o identificador informado.");

            // 3) Calcula a nova quantidade de estoque conforme o tipo de movimento
            int currentStock = inventoryItem.StockQuantity;
            int newStockQuantity = CalculateNewStockQuantity(currentStock, request);

            if (newStockQuantity < 0)
                throw new InvalidOperationException("A operação resultaria em estoque negativo, o que não é permitido.");

            // 4) Atualiza o estoque do item
            inventoryItem.StockQuantity = newStockQuantity;

            // 5) Registra o movimento de estoque
            var movement = new InventoryMovement
            {
                MovementDate = DateTimeOffset.UtcNow,
                Quantity = request.Quantity,
                MovementType = request.MovementType,
                Description = request.Description?.Trim() ?? string.Empty,
                InventoryItem = inventoryItem,
                Administrator = administrator
            };

            inventoryItem.InventoryMovement.Add(movement);

            // 6) Persiste a atualização do item e seu histórico
            await _inventoryItemRepository.UpdateAsync(inventoryItem);

            // 7) Monta o response com o estado atualizado
            return new RegisterInventoryMovementResponse
            {
                InventoryItemId = inventoryItem.Id,
                HealthUnitId = inventoryItem.HealthUnit.Id,
                Name = inventoryItem.Name,
                Description = inventoryItem.Description,
                UnitOfMeasure = inventoryItem.UnitOfMeasure.Value,
                StockQuantity = inventoryItem.StockQuantity
            };
        }

        /// <summary>
        /// Calcula a nova quantidade de estoque com base no tipo de movimentação.
        /// </summary>
        /// <param name="currentStock">Quantidade atual em estoque.</param>
        /// <param name="request">Dados da movimentação.</param>
        /// <returns>Nova quantidade de estoque após a movimentação.</returns>
        private static int CalculateNewStockQuantity(int currentStock, RegisterInventoryMovementRequest request)
        {
            return request.MovementType switch
            {
                InventoryMovementType.Entry => currentStock + request.Quantity,
                InventoryMovementType.Exit => currentStock - request.Quantity,
                InventoryMovementType.Adjustment => request.Quantity, // interpreta Quantity como estoque final
                _ => throw new InvalidOperationException("Tipo de movimentação de estoque inválido.")
            };
        }
    }
}
