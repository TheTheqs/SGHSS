// Tests/TestData/Models/RegisterInventoryItemRequestGenerator.cs

using SGHSS.Application.UseCases.InventoryItems.Register;

namespace SGHSS.Tests.TestData.Models
{
    /// <summary>
    /// Classe utilitária para geração de dados de entrada do caso de uso
    /// de registro de item de estoque (<see cref="RegisterInventoryItemRequest"/>),
    /// fornecendo valores fictícios porém válidos para cenários de teste.
    /// </summary>
    public static class RegisterInventoryItemRequestGenerator
    {
        private static readonly Random Rng = new();

        /// <summary>
        /// Gera um <see cref="RegisterInventoryItemRequest"/> completo e válido,
        /// permitindo sobrescrever qualquer campo para cenários específicos de teste.
        /// </summary>
        /// <param name="providedHealthUnitId">
        /// Identificador da unidade de saúde a ser utilizada.
        /// Caso seja <c>null</c>, um novo <see cref="Guid"/> será gerado.
        /// </param>
        /// <param name="providedName">
        /// Nome do item. Caso não informado, um nome fictício será gerado.
        /// </param>
        /// <param name="providedDescription">
        /// Descrição do item. Caso não informada, uma descrição fictícia será usada.
        /// </param>
        /// <param name="providedUnitOfMeasure">
        /// Unidade de medida. Caso não informada, um alias válido será sorteado.
        /// </param>
        /// <param name="providedStockQuantity">
        /// Quantidade inicial em estoque. Caso seja <c>null</c>, será escolhido um valor de 1 a 100.
        /// </param>
        /// <returns>
        /// Instância preenchida de <see cref="RegisterInventoryItemRequest"/> pronta para uso em testes.
        /// </returns>
        public static RegisterInventoryItemRequest Generate(
            Guid? providedHealthUnitId = null,
            string? providedName = null,
            string? providedDescription = null,
            string? providedUnitOfMeasure = null,
            int? providedStockQuantity = null
        )
        {
            Guid healthUnitId = providedHealthUnitId ?? Guid.NewGuid();
            string name = !string.IsNullOrWhiteSpace(providedName)
                ? providedName!
                : GenerateName();

            string description = !string.IsNullOrWhiteSpace(providedDescription)
                ? providedDescription!
                : GenerateDescription();

            string unitOfMeasure = !string.IsNullOrWhiteSpace(providedUnitOfMeasure)
                ? providedUnitOfMeasure!
                : GenerateRandomUnitOfMeasure();

            int stockQuantity = providedStockQuantity ?? Rng.Next(1, 101);

            return new RegisterInventoryItemRequest
            {
                HealthUnitId = healthUnitId,
                Name = name,
                Description = description,
                UnitOfMeasure = unitOfMeasure,
                StockQuantity = stockQuantity
            };
        }

        /// <summary>
        /// Gera um nome fictício e coerente para um item de estoque.
        /// </summary>
        private static string GenerateName()
        {
            string[] samples =
            {
                "Soro Fisiológico 0,9%",
                "Luvas de Procedimento",
                "Seringa 5ml",
                "Máscara Cirúrgica",
                "Gaze Estéril",
                "Algodão Hidrófilo",
                "Álcool 70%",
                "Kit de Curativos",
                "Compressa Quente",
                "Termômetro Digital"
            };

            return samples[Rng.Next(samples.Length)];
        }

        /// <summary>
        /// Gera descrições coerentes para itens de estoque.
        /// </summary>
        private static string GenerateDescription()
        {
            string[] samples =
            {
                "Item destinado ao uso geral em procedimentos clínicos.",
                "Material descartável utilizado para cuidados ambulatoriais.",
                "Produto essencial para suporte em terapias intravenosas.",
                "Equipamento utilizado para aferição de temperatura corporal.",
                "Material estéril recomendado para curativos simples."
            };

            return samples[Rng.Next(samples.Length)];
        }

        /// <summary>
        /// Sorteia uma unidade de medida válida (alias do VO UnitOfMeasure).
        /// </summary>
        private static string GenerateRandomUnitOfMeasure()
        {
            string[] units =
            {
                "kg", "g", "mg",
                "L", "ml",
                "un", "pct", "cx", "dz",
                "m", "cm", "mm",
                "m2", "m3"
            };

            return units[Rng.Next(units.Length)];
        }
    }
}
