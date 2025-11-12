// Domain/ValueObjects/Cnpj.cs

using System.Text.RegularExpressions;

namespace SGHSS.Domain.ValueObjects
{
    public readonly record struct Cnpj
    {
        // Aceita com e sem máscara (00.000.000/0000-00)
        private static readonly Regex CnpjRegex = new(@"^\d{2}\.?\d{3}\.?\d{3}/?\d{4}-?\d{2}$", RegexOptions.Compiled);

        public string Value { get; }

        public Cnpj(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("CNPJ vazio.", nameof(input));

            var d = Regex.Replace(input, @"\D", "");

            if (d.Length != 14)
                throw new ArgumentException("CNPJ deve conter 14 dígitos.", nameof(input));

            if (d.All(c => c == d[0]))
                throw new ArgumentException("CNPJ inválido (todos os dígitos iguais).", nameof(input));

            if (!CheckDigits(d))
                throw new ArgumentException("CNPJ inválido (dígitos verificadores).", nameof(input));

            Value = d;
        }

        public override string ToString() =>
            Value.Length == 14
                ? $"{Value[..2]}.{Value.Substring(2, 3)}.{Value.Substring(5, 3)}/{Value.Substring(8, 4)}-{Value.Substring(12, 2)}"
                : Value;

        private static bool CheckDigits(string d)
        {
            // Pesos oficiais do CNPJ
            ReadOnlySpan<int> w1 = stackalloc int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            ReadOnlySpan<int> w2 = stackalloc int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            int dv1 = Compute(d.AsSpan(0, 12), w1);
            int dv2 = Compute(d.AsSpan(0, 13), w2);

            return (d[12] - '0') == dv1 && (d[13] - '0') == dv2;

            static int Compute(ReadOnlySpan<char> s, ReadOnlySpan<int> weights)
            {
                int sum = 0;
                for (int i = 0; i < s.Length; i++)
                    sum += (s[i] - '0') * weights[i];

                int m = sum % 11;
                return m < 2 ? 0 : 11 - m;
            }
        }
    }
}
