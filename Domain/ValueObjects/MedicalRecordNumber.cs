// Domain/ValueObjects/MedicalRecordNumber.cs

using System.Text.RegularExpressions;

namespace SGHSS.Domain.ValueObjects
{
    /// <summary>
    /// Número de prontuário (MRN - Medical Record Number).
    /// Aceita letras e dígitos; remove os separadores ('.', '-', '/', espaços).
    /// Normaliza letras para CAIXA ALTA.
    /// Valor final contém somente [A–Z0–9], tamanho entre 6 e 20, preservando zeros à esquerda.
    /// ToString() retorna o Value.
    /// Identificação de unidade médica deve ser implementadoa em outro Value Object ou entidade.
    /// </summary>
    public readonly record struct MedicalRecordNumber
    {
        public string Value { get; }

        public MedicalRecordNumber(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("MRN vazio.", nameof(input));

            // Remove separadores comuns
            var cleaned = Regex.Replace(input.Trim(), @"[.\-/\s]+", "");

            // Letras em caixa alta
            cleaned = cleaned.ToUpperInvariant();

            // Deve conter apenas A–Z e 0–9
            if (!Regex.IsMatch(cleaned, @"^[A-Z0-9]+$"))
                throw new ArgumentException("MRN inválido: use apenas letras A–Z e dígitos 0–9.", nameof(input));

            // Comprimento mínimo/máximo
            if (cleaned.Length < 6 || cleaned.Length > 20)
                throw new ArgumentException("MRN deve conter entre 6 e 20 caracteres.", nameof(input));

            // Rejeita todos os caracteres iguais (ex.: 000000, AAAAAA)
            if (cleaned.All(c => c == cleaned[0]))
                throw new ArgumentException("MRN inválido: todos os caracteres iguais.", nameof(input));

            Value = cleaned;
        }

        public override string ToString() => Value;
    }
}
