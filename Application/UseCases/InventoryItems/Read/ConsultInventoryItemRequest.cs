// Application/UseCases/InventoryItems/Read/ConsultInventoryItemRequest.cs

namespace SGHSS.Application.UseCases.InventoryItems.Consult
{
    /// <summary>
    /// Dados necessários para consultar um item específico do estoque,
    /// garantindo que ele pertença à unidade de saúde informada.
    /// </summary>
    /// <remarks>
    /// Este DTO representa a entrada para o caso de uso de consulta de
    /// item de inventário, permitindo recuperar informações detalhadas
    /// como nome, unidade de medida e quantidade atual em estoque.
    /// </remarks>
    public sealed class ConsultInventoryItemRequest
    {
        /// <summary>
        /// Identificador do item de estoque a ser consultado.
        /// </summary>
        public Guid InventoryItemId { get; set; }

        /// <summary>
        /// Identificador da unidade de saúde à qual o item deve estar associado.
        /// </summary>
        public Guid HealthUnitId { get; set; }
    }
}
