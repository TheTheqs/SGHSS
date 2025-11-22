// Tests/TestData/Models/BedGenerator.cs

using SGHSS.Application.UseCases.Common;
using SGHSS.Domain.Enums;

namespace SGHSS.Tests.TestData.Models
{
    /// <summary>
    /// Classe utilitária para geração de dados de leitos (BedDto)
    /// utilizados em testes de casos de uso relacionados a leitos.
    /// </summary>
    /// <remarks>
    /// Este gerador cria BedDto válidos com numeração simulada,
    /// permitindo testar operações de adição, remoção e manipulação
    /// de leitos em unidades de saúde.
    /// </remarks>
    public static class BedGenerator
    {
        private static readonly Random Rng = new();

        /// <summary>
        /// Gera um objeto <see cref="BedDto"/> válido para uso em testes.
        /// </summary>
        /// <param name="type">
        /// Tipo do leito. Caso não informado, será escolhido aleatoriamente.
        /// </param>
        /// <param name="status">
        /// Status do leito. Caso não informado, será escolhido aleatoriamente,
        /// exceto pelo uso frequente do status Available para testes comuns.
        /// </param>
        /// <returns>Instância de <see cref="BedDto"/> preenchida com valores válidos.</returns>
        public static BedDto GenerateBed(
            BedType? type = null,
            BedStatus? status = null)
        {
            var resolvedType = type ?? GetRandomBedType();
            var resolvedStatus = status ?? GetRandomBedStatus();

            return new BedDto
            {
                BedNumber = GenerateBedNumber(resolvedType),
                Type = resolvedType,
                Status = resolvedStatus
            };
        }

        /// <summary>
        /// Gera um número de leito simulando padrões reais utilizados em hospitais.
        /// </summary>
        /// <param name="type">Tipo do leito (utilizado para prefixos reais).</param>
        /// <returns>Uma string representando um número de leito.</returns>
        private static string GenerateBedNumber(BedType type)
        {
            // Faixa numérica realista (entre 1 e 999)
            int num = Rng.Next(1, 999);

            // Prefixos comuns por tipo
            string prefix = type switch
            {
                BedType.ICU => "UTI",
                BedType.Maternity => "MAT",
                BedType.Pediatric => "PED",
                BedType.Isolation => "ISO",
                _ => "BED"
            };

            return $"{prefix}-{num:D3}";
        }

        /// <summary>
        /// Sorteia um tipo de leito dentre os valores definidos no enum.
        /// </summary>
        private static BedType GetRandomBedType()
        {
            var values = Enum.GetValues(typeof(BedType));
            return (BedType)values.GetValue(Rng.Next(values.Length))!;
        }

        /// <summary>
        /// Sorteia um status dentre os valores definidos no enum.
        /// </summary>
        private static BedStatus GetRandomBedStatus()
        {
            var values = Enum.GetValues(typeof(BedStatus));
            return (BedStatus)values.GetValue(Rng.Next(values.Length))!;
        }
    }
}
