// Domain/ValueObjects/TimeZone.cs

using System.Text.RegularExpressions;

namespace SGHSS.Domain.ValueObjects
{
    /// <summary>
    /// Representa um fuso horário (IANA ou offset UTC).
    /// Case-sensitive, mas aceita apenas IANA válidos ou offsets no formato UTC±HH:MM.
    /// Normaliza representações equivalentes (UTC, Z, Etc/UTC, offsets).
    /// Não normaliza sempre para Iana ou Offset, mantém o formato de entrada canônico para evitar desvios em horários de verão.
    /// </summary>
    public readonly record struct TimeZone
    {
        public string Value { get; }

        private static readonly Regex OffsetRegex = new(@"^(UTC)?([+-])(\d{1,2})(:?(\d{2}))?$", RegexOptions.Compiled);
        private static readonly Regex IanaRegex = new(@"^[A-Za-z0-9_\-]+(/[A-Za-z0-9_\-]+)*$", RegexOptions.Compiled);

        public TimeZone(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Fuso horário vazio.", nameof(input));

            input = input.Trim();

            // Canonicalização de UTC "simples"
            if (input is "UTC" or "Etc/UTC" or "Z")
            {
                Value = "Etc/UTC";
                return;
            }

            // Canonicalização de offsets
            var m = OffsetRegex.Match(input);
            if (m.Success)
            {
                var sign = m.Groups[2].Value;
                var hours = int.Parse(m.Groups[3].Value);
                var minutes = m.Groups[5].Success ? int.Parse(m.Groups[5].Value) : 0;

                if (hours > 14 || minutes > 59)
                    throw new ArgumentException("Offset UTC inválido.", nameof(input));

                Value = $"UTC{sign}{hours:D2}:{minutes:D2}";
                return;
            }

            // Caso IANA (ex.: America/Sao_Paulo)
            if (IanaRegex.IsMatch(input))
            {
                Value = input; // case-sensitive
                return;
            }

            throw new ArgumentException("Formato de fuso horário inválido.", nameof(input));
        }

        public override string ToString() => Value;
    }
}
