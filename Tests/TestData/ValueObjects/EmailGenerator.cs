// Tests/TestData/ValueObjects/EmailGenerator.cs

using System.Text;

namespace SGHSS.Tests.TestData.ValueObjects
{

    /// <summary>
    /// Classe utilitária para gerar emails válidos segundo as regras definidas pelo VO Email.
    /// </summary>
    public static class EmailGenerator
    {
        // Domínios válidos
        private static readonly List<string> Domains = new()
    {
        "gmail.com",
        "hotmail.com",
        "outlook.com",
        "yahoo.com",
        "uol.com.br",
        "bol.com.br"
    };

        private static readonly Random Rng = new();

        /// <summary>
        /// Gera um email válido com base em um nome completo.
        /// O email sempre segue o formato aceito pelo VO Email:
        /// - Apenas 1 @
        /// - Apenas 1 .
        /// - Sem espaços
        /// - Partes válidas antes e depois do @
        /// - Um dígito aleatório antes do @ para reduzir colisões
        /// </summary>
        /// <param name="fullName">O nome completo fornecido pelo NameGenerator.</param>
        /// <returns>Um email gerado no formato aceitável.</returns>
        public static string GenerateEmail(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Nome completo inválido.");

            // Normaliza nome
            var parts = fullName
                .Trim()
                .ToLowerInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            if (parts.Count < 2)
                throw new ArgumentException("Nome completo deve ter ao menos nome e sobrenome.");

            // Pega primeiro nome + último sobrenome
            string first = parts.First();
            string last = parts.Last();

            // Remove acentos para evitar emails quebrados
            first = RemoveAccents(first);
            last = RemoveAccents(last);

            // Gera número aleatório para reduzir colisões
            int randomDigits = Rng.Next(0, 999);

            // Escolhe um domínio
            string domain = Domains[Rng.Next(Domains.Count)];
            string email = $"{first}{last}{randomDigits}@{domain}";

            return email;
        }

        /// <summary>
        /// Remove acentos e caracteres especiais comuns em nomes brasileiros.
        /// </summary>
        private static string RemoveAccents(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var unicode = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicode != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }

}
