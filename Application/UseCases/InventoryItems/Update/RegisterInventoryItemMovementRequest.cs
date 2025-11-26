// Application/UseCases/InvenotryItem/Update/RegisterInventoryItemMovementRequest

using SGHSS.Domain.Enums;

namespace SGHSS.Application.UseCases.InventoryItems.Update
{
    /// <summary>
    /// Request para registrar um movimento de estoque (entrada, saída ou ajuste)
    /// associado a um item de inventário.
    /// </summary>
    public class RegisterInventoryMovementRequest
    {
        /// <summary>
        /// Identificador do item de inventário cujo estoque será alterado.
        /// </summary>
        public Guid InventoryItemId { get; set; }

        /// <summary>
        /// Identificador da unidade de saúde à qual o item pertence.
        /// </summary>
        public Guid HealthUnitId { get; set; }

        /// <summary>
        /// Quantidade do movimento. Deve ser positiva.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Tipo do movimento (entrada, saída ou ajuste).
        /// </summary>
        public InventoryMovementType MovementType { get; set; }

        /// <summary>
        /// Identificador do administrador responsável pela movimentação.
        /// </summary>
        public Guid AdministratorId { get; set; }

        /// <summary>
        /// Descrição opcional da movimentação (motivo ou observações).
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}
