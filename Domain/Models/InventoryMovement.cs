// Domain/Models/InventoryMovement.cs

using SGHSS.Domain.Enums;

namespace SGHSS.Domain.Models
{
    /// <summary>
    /// Representa um registro de movimento de estoque, como a adição ou remoção de itens,
    /// dentro do sistema de inventário.
    /// </summary>
    /// <remarks>Um movimento de estoque rastreia alterações nas quantidades de itens, incluindo o tipo de
    /// movimento, a quantidade afetada, a data da transação e entidades relacionadas, como o item de
    /// inventário e o administrador responsável. Esta classe é normalmente usada para auditar mudanças
    /// no estoque e manter registros precisos.</remarks>
    public class InventoryMovement
    {
        public Guid Id { get; set; }
        public DateTimeOffset MovementDate { get; set; }
        public int Quantity { get; set; }
        public InventoryMovementType MovementType { get; set; }
        public string Description { get; set; } = string.Empty;

        // Relacionamentos
        public InventoryItem InventoryItem { get; set; } = null!;
        public Administrator Administrator { get; set; } = null!;

        // Construtor padrão
        public InventoryMovement() { }
    }
}
