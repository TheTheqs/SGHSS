// Tests/TestData/ValueObjects/ProfessionalLicenseGenerator.cs

using SGHSS.Domain.ValueObjects;

namespace SGHSS.Tests.TestData.ValueObjects
{
    /// <summary>
    /// Classe utilitária para geração de registros profissionais válidos,
    /// compatíveis com o Value Object <see cref="ProfessionalLicense"/>.
    /// </summary>
    public static class ProfessionalLicenseGenerator
    {
        private static readonly Random Rng = new();

        // Mesmos conselhos suportados pelo VO
        private static readonly string[] CouncilsWhitelist =
        {
            "CRM",    // Medicina
            "COREN",  // Enfermagem
            "CRP",    // Psicologia
            "CRF",    // Farmácia
            "CRO",    // Odontologia
            "CREFITO",// Fisioterapia/Terapia Ocupacional
            "CRN",    // Nutrição
            "CREFONO" // Fonoaudiologia
        };

        // Mesmas UFs do VO
        private static readonly string[] UFs =
        {
            "AC","AL","AP","AM","BA","CE","DF","ES","GO","MA","MT","MS","MG",
            "PA","PB","PR","PE","PI","RJ","RN","RS","RO","RR","SC","SP","SE","TO"
        };

        /// <summary>
        /// Gera um registro profissional fictício no formato canônico:
        /// "COUNCIL-UF NUMBER" (ex.: "CRM-SP 123456").
        /// </summary>
        /// <param name="council">
        /// Opcional: conselho a ser utilizado (ex.: "CRM", "CRP"). Caso não informado, será escolhido aleatoriamente.
        /// </param>
        /// <param name="uf">
        /// Opcional: UF a ser utilizada (ex.: "SP", "RJ"). Caso não informada, será escolhida aleatoriamente.
        /// </param>
        /// <param name="numberLength">
        /// Opcional: tamanho do número de registro (entre 4 e 7 dígitos). Caso não informado, será escolhido aleatoriamente.
        /// </param>
        /// <returns>Uma string válida que pode ser utilizada para instanciar <see cref="ProfessionalLicense"/>.</returns>
        public static string GenerateLicense(string? council = null, string? uf = null, int? numberLength = null)
        {
            string chosenCouncil = string.IsNullOrWhiteSpace(council)
                ? PickRandomCouncil()
                : council.ToUpperInvariant();

            string chosenUf = string.IsNullOrWhiteSpace(uf)
                ? PickRandomUf()
                : uf.ToUpperInvariant();

            int effectiveLength = numberLength.HasValue
                ? Math.Clamp(numberLength.Value, 4, 7)
                : Rng.Next(4, 8); // 4 a 7

            string number = GenerateNumber(effectiveLength);

            string value = $"{chosenCouncil}-{chosenUf} {number}";

            return value;
        }

        /// <summary>
        /// Escolhe um conselho suportado aleatoriamente.
        /// </summary>
        private static string PickRandomCouncil()
        {
            return CouncilsWhitelist[Rng.Next(CouncilsWhitelist.Length)];
        }

        /// <summary>
        /// Escolhe uma UF válida aleatoriamente.
        /// </summary>
        private static string PickRandomUf()
        {
            return UFs[Rng.Next(UFs.Length)];
        }

        /// <summary>
        /// Gera uma sequência numérica com o tamanho desejado (entre 4 e 7 dígitos).
        /// </summary>
        private static string GenerateNumber(int length)
        {
            var digits = new char[length];

            for (int i = 0; i < length; i++)
            {
                digits[i] = (char)('0' + Rng.Next(0, 10));
            }

            return new string(digits);
        }
    }
}
