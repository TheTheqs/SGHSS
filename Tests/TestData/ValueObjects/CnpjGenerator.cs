// Tests/TestData/ValueObjects/CnpjGenerator.cs


namespace SGHSS.Tests.TestData.ValueObjects
{
    /// <summary>
    /// Classe utilitária para geração de CNPJs válidos, compatíveis com o VO Cnpj.
    /// </summary>
    public static class CnpjGenerator
    {
        private static readonly Random Rng = new();

        /// <summary>
        /// Gera um CNPJ válido segundo as regras oficiais de cálculo.
        /// Pode retornar com máscara (##.###.###/####-##) ou apenas dígitos.
        /// </summary>
        /// <param name="withMask">Define se o CNPJ deve ser retornado com máscara.</param>
        /// <returns>Uma string contendo um CNPJ válido.</returns>
        public static string GenerateCnpj(bool withMask = false)
        {
            int[] digits = new int[14];

            // Gera os 12 primeiros dígitos (raiz + filial)
            for (int i = 0; i < 12; i++)
            {
                digits[i] = Rng.Next(0, 10);
            }

            // Evita CNPJ inválido por todos dígitos iguais
            if (digits.All(d => d == digits[0]))
            {
                digits[11] = (digits[0] + 1) % 10;
            }

            // Pesos oficiais
            int[] w1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };           // dv1
            int[] w2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };       // dv2

            // Calcula dv1
            digits[12] = ComputeCheckDigit(digits, 12, w1);

            // Calcula dv2
            digits[13] = ComputeCheckDigit(digits, 13, w2);

            // Monta o número
            var raw = string.Concat(digits.Select(d => d.ToString()));

            if (!withMask)
                return raw;

            // Máscara: 00.000.000/0000-00
            return $"{raw[..2]}.{raw.Substring(2, 3)}.{raw.Substring(5, 3)}/{raw.Substring(8, 4)}-{raw.Substring(12, 2)}";
        }

        /// <summary>
        /// Calcula um dígito verificador seguindo o algoritmo oficial do CNPJ.
        /// </summary>
        private static int ComputeCheckDigit(int[] digits, int len, int[] weights)
        {
            int sum = 0;

            for (int i = 0; i < len; i++)
            {
                sum += digits[i] * weights[i];
            }

            int m = sum % 11;
            return m < 2 ? 0 : 11 - m;
        }
    }
}
