// Application/UseCases/InventoryItems/Register/RegisterInventoryItemResponse.cs

using System;

namespace SGHSS.Application.UseCases.InventoryItems.Register
{
    /// <summary>
    /// Representa o resultado do cadastro de um novo item de estoque.
    /// </summary>
    /// <remarks>
    /// Este DTO é retornado após a criação bem-sucedida de um <c>InventoryItem</c>
    /// na base de dados, trazendo as informações consolidadas do registro.
    /// </remarks>
    public sealed class RegisterInventoryItemResponse
    {
        /// <summary>
        /// Identificador único do item de estoque recém-cadastrado.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nome do item de estoque.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descrição detalhada do item de estoque.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Unidade de medida do item em seu código canônico
        /// (valor retornado pelo value object <c>UnitOfMeasure</c>, ex.: "KG", "UN", "ML").
        /// </summary>
        public string UnitOfMeasure { get; set; } = string.Empty;

        /// <summary>
        /// Quantidade atual em estoque registrada para o item.
        /// </summary>
        public int StockQuantity { get; set; }

        /// <summary>
        /// Identificador da unidade de saúde à qual o item de estoque está associado.
        /// </summary>
        public Guid HealthUnitId { get; set; }
    }
}
