// Application/UseCases/InventoryItems/Read/ConsultInventoryItemResponse.cs

namespace SGHSS.Application.UseCases.InventoryItems.Consult
{
    /// <summary>
    /// Dados retornados pela consulta de um item de estoque.
    /// </summary>
    /// <remarks>
    /// Contém as informações essenciais do item, incluindo quantidade
    /// atual em estoque e unidade de medida normalizada.
    /// </remarks>
    public sealed class ConsultInventoryItemResponse
    {
        /// <summary>
        /// Identificador único do item consultado.
        /// </summary>
        public Guid InventoryItemId { get; set; }

        /// <summary>
        /// Nome do item de estoque.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descrição do item de estoque.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Unidade de medida canônica utilizada pelo item
        /// (ex.: "KG", "ML", "UN").
        /// </summary>
        public string UnitOfMeasure { get; set; } = string.Empty;

        /// <summary>
        /// Quantidade atual disponível em estoque.
        /// </summary>
        public int StockQuantity { get; set; }

        /// <summary>
        /// Identificador da unidade de saúde à qual o item pertence.
        /// </summary>
        public Guid HealthUnitId { get; set; }
    }
}
