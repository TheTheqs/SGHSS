// Tests/TestData/ValueObjects/CpfGenerator.cs


namespace SGHSS.Tests.TestData.ValueObjects
{
    /// <summary>
    /// Classe utilitária para geração de CPFs válidos segundo as regras do VO Cpf.
    /// </summary>
    public static class CpfGenerator
    {
        private static readonly Random Rng = new();

        /// <summary>
        /// Gera um CPF válido.
        /// Pode retornar com máscara (###.###.###-##) ou apenas com dígitos.
        /// </summary>
        /// <param name="withMask">Define se o CPF será retornado com máscara.</param>
        /// <returns>Uma string contendo um CPF válido.</returns>
        public static string GenerateCpf(bool withMask = false)
        {
            int[] digits = new int[11];

            // Gera os 9 primeiros dígitos
            for (int i = 0; i < 9; i++)
            {
                digits[i] = Rng.Next(0, 10);
            }

            // Evita que todos os dígitos sejam iguais (CPF inválido)
            if (digits.All(d => d == digits[0]))
            {
                // Força pelo menos um dígito diferente
                digits[8] = (digits[0] + 1) % 10;
            }

            // Calcula o primeiro dígito verificador (dv1)
            digits[9] = ComputeCheckDigit(digits, 9, 10);

            // Calcula o segundo dígito verificador (dv2)
            digits[10] = ComputeCheckDigit(digits, 10, 11);

            // Monta string com os 11 dígitos
            string raw = string.Concat(digits.Select(d => d.ToString()));

            if (!withMask)
                return raw;

            // Aplica máscara ###.###.###-##
            return $"{raw[..3]}.{raw.Substring(3, 3)}.{raw.Substring(6, 3)}-{raw.Substring(9, 2)}";
        }

        /// <summary>
        /// Calcula um dígito verificador de CPF usando o mesmo algoritmo do VO.
        /// </summary>
        /// <param name="digits">Array de dígitos do CPF.</param>
        /// <param name="length">Quantidade de dígitos considerados (9 para dv1, 10 para dv2).</param>
        /// <param name="weightStart">Peso inicial (10 para dv1, 11 para dv2).</param>
        /// <returns>O dígito verificador calculado.</returns>
        private static int ComputeCheckDigit(int[] digits, int length, int weightStart)
        {
            int sum = 0;
            int weight = weightStart;

            for (int i = 0; i < length; i++)
            {
                sum += digits[i] * weight--;
            }

            int m = sum % 11;
            return m < 2 ? 0 : 11 - m;
        }
    }

}
