// Tests/TestData/ValueObjects/PhoneGenerator.cs

namespace SGHSS.Tests.TestData.ValueObjects
{
    /// <summary>
    /// Classe utilitária para gerar telefones brasileiros válidos,
    /// compatíveis com o VO Phone.
    /// </summary>
    public static class PhoneGenerator
    {
        private static readonly Random Rng = new();

        /// <summary>
        /// Gera um telefone brasileiro válido (fixo ou celular).
        /// O telefone é retornado sem máscara (o VO aplica no ToString).
        /// </summary>
        /// <returns>Instância válida de Phone.</returns>
        public static string GeneratePhone()
        {
            string ddd = GenerateDdd();

            // Decide se será fixo (10 dígitos) ou celular (11 dígitos)
            bool celular = Rng.NextDouble() < 0.7; // 70% de chance de celular (mais realista)

            string number = celular
                ? GenerateMobileNumber() // 9 + 4 + 4
                : GenerateLandlineNumber(); // 4 + 4

            string full = ddd + number;

            // Garante que não seja todos iguais
            if (full.All(c => c == full[0]))
                return GeneratePhone(); // recursão segura e rara

            return full;
        }

        /// <summary>
        /// Gera um DDD válido entre 11 e 99 (DDDs 01–10 não existem).
        /// </summary>
        private static string GenerateDdd()
        {
            int d = Rng.Next(11, 100);
            return d.ToString("00");
        }

        /// <summary>
        /// Gera linha móvel brasileira:
        /// Formato: 9XXXX XXXX → total 9 dígitos.
        /// </summary>
        private static string GenerateMobileNumber()
        {
            int firstPart = Rng.Next(10000, 99999); // 5 dígitos
            int lastPart = Rng.Next(1000, 9999);    // 4 dígitos
            return $"9{firstPart}{lastPart}".Substring(0, 9);
        }

        /// <summary>
        /// Gera linha fixa:
        /// Formato: XXXX XXXX (8 dígitos).
        /// </summary>
        private static string GenerateLandlineNumber()
        {
            int firstPart = Rng.Next(2000, 5999); // Faixa mais comum
            int lastPart = Rng.Next(1000, 9999);
            return $"{firstPart}{lastPart}";
        }
    }

}
