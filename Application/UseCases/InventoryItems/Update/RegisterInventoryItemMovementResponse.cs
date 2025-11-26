// Application/UseCases/InvenotryItem/Update/RegisterInventoryItemMovementResponse.cs

namespace SGHSS.Application.UseCases.InventoryItems.Update
{
    /// <summary>
    /// Representa a resposta após o registro de uma movimentação de estoque,
    /// retornando o estado atualizado do item de inventário.
    /// </summary>
    public class RegisterInventoryMovementResponse
    {
        public Guid InventoryItemId { get; set; }
        public Guid HealthUnitId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
    }
}
