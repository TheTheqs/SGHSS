// Application/UseCases/InventoryItems/Register/RegisterInventoryItemRequest.cs

using System;

namespace SGHSS.Application.UseCases.InventoryItems.Register
{
    /// <summary>
    /// Dados necessários para registrar um novo item de estoque na unidade de saúde.
    /// </summary>
    /// <remarks>
    /// Este DTO representa a entrada do caso de uso de cadastro de item de inventário.
    /// A unidade de medida é enviada como texto (alias ou código) e será convertida
    /// para o value object <c>UnitOfMeasure</c> na camada de domínio.
    /// </remarks>
    public sealed class RegisterInventoryItemRequest
    {
        /// <summary>
        /// Identificador da unidade de saúde responsável por este item de estoque.
        /// </summary>
        public Guid HealthUnitId { get; set; }

        /// <summary>
        /// Nome do item de estoque (ex.: "Luvas de procedimento", "Soro fisiológico 0,9%").
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descrição detalhada do item de estoque, incluindo observações relevantes.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Unidade de medida do item, informada como texto.
        /// Pode ser um alias conhecido (ex.: "kg", "litro", "unidade")
        /// ou um código customizado válido para o value object <c>UnitOfMeasure</c>.
        /// </summary>
        public string UnitOfMeasure { get; set; } = string.Empty;

        /// <summary>
        /// Quantidade inicial em estoque para o item cadastrado.
        /// </summary>
        public int StockQuantity { get; set; }
    }
}
