// Domain/ValueObjects/UnitOfMeasure.cs

using System.Text.RegularExpressions;

namespace SGHSS.Domain.ValueObjects
{
    /// <summary>
    /// Unidade de medida para estoque, com aliases comuns e suporte a códigos customizados.
    /// Normaliza aliases conhecidos para um código canônico (ex.: "kg", "quilograma" -> "KG").
    /// Permite código customizado [A-Z0-9] de 1 a 6 caracteres (ex.: "KIT01").
    /// Entradas fora do escopo de aliases não geram exceção desde que respeitem o formato.
    /// Rejeita expressões compostas (ex.: "kg/m", "m^2" com símbolos; use "M2" ou "metroquadrado").
    /// ToString() retorna o código canônico (Value).
    /// </summary>
    public readonly record struct UnitOfMeasure
    {
        public string Value { get; }

        private static readonly Regex CompositeMarks = new(@"[/*^×÷·]", RegexOptions.Compiled);
        private static readonly Regex SepClean = new(@"[.\-\s_]+", RegexOptions.Compiled);
        private static readonly Dictionary<string, string> Aliases = BuildAliases();

        public UnitOfMeasure(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Unidade vazia.", nameof(input));

            var original = input.Trim();

            // Rejeita unidades compostas (ex.: kg/m, m^2 com símbolo '^', etc.)
            if (CompositeMarks.IsMatch(original))
                throw new ArgumentException("Unidade composta não suportada. Use um código simples (ex.: 'M2').", nameof(input));

            // Normaliza para comparar aliases:
            // - remove separadores simples
            // - lower para matching
            var simplified = SepClean.Replace(original, "");
            var key = simplified.ToLowerInvariant();

            if (Aliases.TryGetValue(key, out var canonical))
            {
                Value = canonical; // código canônico (ex.: "KG", "UN", "ML", "M2", ...)
                return;
            }

            // Se não é alias conhecido, aceitar como código custom (1..6 A-Z0-9)
            var code = simplified.ToUpperInvariant();

            if (!Regex.IsMatch(code, @"^[A-Z0-9]{1,6}$"))
                throw new ArgumentException("Código de unidade inválido. Use de 1 a 6 chars A–Z/0–9.", nameof(input));

            Value = code;
        }

        public override string ToString() => Value;

        private static Dictionary<string, string> BuildAliases()
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            void Map(string canonical, params string[] aliases)
            {
                foreach (var a in aliases)
                {
                    var k = SepClean.Replace(a.Trim(), "").ToLowerInvariant();
                    if (!dict.ContainsKey(k))
                        dict[k] = canonical;
                }
            }

            // Massa
            Map("KG", "kg", "kilo", "kilogram", "kilogramme", "quilograma", "quilo");
            Map("G", "g", "gram", "grama", "gramas");
            Map("MG", "mg", "miligram", "miligrama", "miligramas");

            // Volume
            Map("L", "l", "lt", "litro", "litros", "liter");
            Map("ML", "ml", "mililitro", "mililitre", "milliliter", "mililitros");

            // Comprimento / área / volume geométrico
            Map("M", "m", "metro", "metros", "meter");
            Map("CM", "cm", "centimetro", "centímetro", "centimetros", "centímetros");
            Map("MM", "mm", "milimetro", "milímetro", "milimetros", "milímetros");

            Map("M2", "m2", "metroquadrado", "metroquadrados", "metrosquadrados", "squaremeter", "sqm");
            Map("M3", "m3", "metrocubico", "metro cúbico", "metroscubicos", "cubicmeter", "cubicmeters", "cbm");

            // Unidades de contagem
            Map("UN", "un", "und", "unidade", "unidades", "unit", "units", "pc", "pcs", "peça", "peças");
            Map("DZ", "dz", "duzia", "dúzia", "dozen");
            Map("PCT", "pct", "pacote", "pack", "packet");

            // Embalagens comuns
            Map("CX", "cx", "caixa", "box");
            Map("FD", "fd", "fardo", "bundle");

            // Tempo (se usar)
            Map("H", "h", "hr", "hora", "horas");
            Map("MIN", "min", "minuto", "minutos");

            return dict;
        }
    }
}
