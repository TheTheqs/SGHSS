// Domain/ValueObjects/BedCode.cs

using System.Text.RegularExpressions;

namespace SGHSS.Domain.ValueObjects
{
    /// <summary>
    /// Código de leito hospitalar (ex.: "UTI01A23", "ENF203B").
    /// Aceita letras e dígitos; remove separadores ('.', '-', '_', '/', espaços).
    /// Normaliza letras para CAIXA ALTA.
    /// Valor final contém somente [A–Z0–9], tamanho entre 2 e 20, preservando zeros à esquerda.
    /// ToString() retorna o Value.
    /// </summary>
    public readonly record struct BedCode
    {
        public string Value { get; }

        public BedCode(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Código de leito vazio.", nameof(input));

            // Remove separadores comuns
            var cleaned = Regex.Replace(input.Trim(), @"[.\-_/ \t]+", "");

            // Letras em caixa alta
            cleaned = cleaned.ToUpperInvariant();

            // Deve conter apenas A–Z e 0–9
            if (!Regex.IsMatch(cleaned, @"^[A-Z0-9]+$"))
                throw new ArgumentException("Código de leito inválido: use apenas letras A–Z e dígitos 0–9.", nameof(input));

            // Comprimento mínimo/máximo
            if (cleaned.Length < 2 || cleaned.Length > 20)
                throw new ArgumentException("Código de leito deve conter entre 2 e 20 caracteres.", nameof(input));

            // Rejeita todos os caracteres iguais (ex.: AAAAAA, 000000)
            if (cleaned.All(c => c == cleaned[0]))
                throw new ArgumentException("Código de leito inválido: todos os caracteres iguais.", nameof(input));

            Value = cleaned;
        }

        public override string ToString() => Value;
    }
}
