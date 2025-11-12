// Domain/ValueObjects/HashDigest.cs

using System.Text.RegularExpressions;

namespace SGHSS.Domain.ValueObjects
{
    /// <summary>
    /// Hash necessário para verificação de integridade de dados na assinatura de consentimento e demais documentos.
    /// Hash (digest) hexadecimal para verificação de integridade (ex.: SHA-256).
    /// Aceita string hex (case-insensitive), com ou sem "0x" e com separadores (:, -, espaços).
    /// Normaliza para minúsculas e remove separadores, resultando em apenas [0-9a-f].
    /// Tamanhos suportados (hex): 32, 40, 56, 64, 96, 128.
    /// ToString() retorna o Value.
    /// </summary>

    public readonly record struct HashDigest
    {
        public string Value { get; }

        private static readonly int[] AllowedHexLengths = { 32, 40, 56, 64, 96, 128 };
        private static readonly Regex HexRegex = new("^[0-9a-f]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public HashDigest(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Hash vazio.", nameof(input));

            var s = input.Trim();

            // Remove prefixo 0x/0X se presente
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                s = s.Substring(2);

            // Remove separadores comuns: espaços, dois-pontos e hífens
            s = Regex.Replace(s, @"[\s:\-]+", "");

            // Normaliza para minúsculas
            s = s.ToLowerInvariant();

            if (s.Length == 0)
                throw new ArgumentException("Hash inválido.", nameof(input));

            // Verifica charset (somente hex)
            if (!HexRegex.IsMatch(s))
                throw new ArgumentException("Hash inválido: use apenas dígitos hexadecimais.", nameof(input));

            // Verifica tamanho suportado
            bool lengthOk = false;
            foreach (var len in AllowedHexLengths)
                if (s.Length == len) { lengthOk = true; break; }

            if (!lengthOk)
                throw new ArgumentException("Hash com comprimento não suportado.", nameof(input));

            Value = s;
        }

        public override string ToString() => Value;
    }
}
