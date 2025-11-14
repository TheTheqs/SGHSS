// Domain/Models/InventoryItem.cs

using SGHSS.Domain.ValueObjects;

namespace SGHSS.Domain.Models
{
    /// <summary>
    /// Representa um item no estoque, incluindo sua identificação, descrição, unidade de medida,
    /// quantidade em estoque e as unidades de saúde e movimentos de estoque relacionados.
    /// </summary>
    /// <remarks>Use esta classe para modelar itens individuais rastreados dentro de um sistema de inventário,
    /// associando cada item à sua unidade de medida e à unidade de saúde responsável por ele. A classe também
    /// mantém uma coleção de movimentos de estoque para acompanhar as alterações nos níveis de estoque ao
    /// longo do tempo.</remarks>

    public class InventoryItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public UnitOfMeasure UnitOfMeasure { get; set; }
        public int StockQuantity { get; set; }

        // Relacionamentos
        public HealthUnit HealthUnit { get; set; } = null!;
        public ICollection<InventoryMovement> InventoryMovement { get; set; } = new List<InventoryMovement>();

        // Construtor padrão
        public InventoryItem() { }
    }
}
