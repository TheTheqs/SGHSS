// Domain/ValueObjects/Cpf.cs

using System.Text.RegularExpressions;
namespace SGHSS.Domain.ValueObjects
{
    public readonly record struct Cpf
    {
        private static readonly Regex CpfRegex = new(@"^\d{3}\.?\d{3}\.?\d{3}-?\d{2}$", RegexOptions.Compiled);
        public string Value { get; }

        public Cpf(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("CPF vazio.", nameof(input));

            var d = Regex.Replace(input, @"\D", "");

            if (d.Length != 11)
                throw new ArgumentException("CPF deve conter 11 dígitos.", nameof(input));

            if (d.All(c => c == d[0]))
                throw new ArgumentException("CPF inválido (todos os dígitos iguais).", nameof(input));

            if (!CheckDigits(d))
                throw new ArgumentException("CPF inválido (dígitos verificadores).", nameof(input));

            Value = d;
        }

        public override string ToString() =>
            Value.Length == 11
                ? $"{Value[..3]}.{Value.Substring(3, 3)}.{Value.Substring(6, 3)}-{Value.Substring(9, 2)}"
                : Value;

        private static bool CheckDigits(string d)
        {
            int dv1 = Compute(d.AsSpan(0, 9), 10);
            int dv2 = Compute(d.AsSpan(0, 10), 11);
            return (d[9] - '0') == dv1 && (d[10] - '0') == dv2;

            static int Compute(ReadOnlySpan<char> s, int w)
            {
                int sum = 0;
                for (int i = 0; i < s.Length; i++) sum += (s[i] - '0') * w--;
                int m = sum % 11;
                return m < 2 ? 0 : 11 - m;
            }
        }
    }
}
