// Domain/ValueObjects/Phone.cs

using System.Text.RegularExpressions;

namespace SGHSS.Domain.ValueObjects
{
    /// <summary>
    /// Representa um número de telefone válido no Brasil.
    /// Aceita com ou sem máscara, armazenando apenas os dígitos, mas recusa explicitamente letras.
    /// O ToString() retorna o telefone formatado com máscara.
    /// </summary>
    public readonly record struct Phone
    {
        // Aceita formatos como:
        // 11987654321, (11) 98765-4321, (11) 9876-4321, etc.
        private static readonly Regex PhoneRegex = new(@"^\(?\d{2}\)?\s?\d{4,5}-?\d{4}$", RegexOptions.Compiled);
        public string Value { get; }

        public Phone(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Telefone vazio.", nameof(input));

            // Recusa letras imediatamente
            if (Regex.IsMatch(input, "[a-zA-Z]"))
                throw new ArgumentException("Telefone inválido: use apenas números e símbolos de formatação.", nameof(input));

            // Remove tudo que não for número
            var d = Regex.Replace(input, @"\D", "");

            if (d.Length < 10 || d.Length > 11)
                throw new ArgumentException("Telefone deve conter 10 ou 11 dígitos.", nameof(input));

            if (d.All(c => c == d[0]))
                throw new ArgumentException("Telefone inválido (todos os dígitos iguais).", nameof(input));

            // Exemplo: DDD + número = 11987654321
            Value = d;
        }

        public override string ToString()
        {
            if (Value.Length == 10)
                return $"({Value[..2]}) {Value.Substring(2, 4)}-{Value.Substring(6, 4)}";

            if (Value.Length == 11)
                return $"({Value[..2]}) {Value.Substring(2, 5)}-{Value.Substring(7, 4)}";

            return Value;
        }
    }
}
