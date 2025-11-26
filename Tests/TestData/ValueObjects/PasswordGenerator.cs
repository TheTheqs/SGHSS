// Tests/TestData/ValueObjects/PasswordGenerator.cs

using SGHSS.Domain.ValueObjects;

namespace SGHSS.Tests.TestData.ValueObjects
{
    /// <summary>
    /// Classe utilitária para geração de senhas válidas segundo as regras do VO Password.
    /// Gera strings aleatórias contendo letras e dígitos e permite criar o VO já instanciado.
    /// </summary>
    public static class PasswordGenerator
    {
        private static readonly Random Rng = new();

        private const string Letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Digits = "0123456789";

        /// <summary>
        /// Gera uma senha válida na forma pura (string),
        /// contendo letras e números e respeitando o tamanho mínimo.
        /// </summary>
        /// <param name="length">Tamanho mínimo da senha (padrão: 8).</param>
        public static string GenerateRawPassword(int length = 8)
        {
            if (length < 6)
                length = 6;

            // Garantimos pelo menos 1 letra e 1 número
            char requiredLetter = Letters[Rng.Next(Letters.Length)];
            char requiredDigit = Digits[Rng.Next(Digits.Length)];

            // Gera caracteres aleatórios
            var chars = new List<char> { requiredLetter, requiredDigit };

            string all = Letters + Digits;

            while (chars.Count < length)
                chars.Add(all[Rng.Next(all.Length)]);

            // Embaralha para não ficar letra+número fixo
            return new string(chars.OrderBy(_ => Rng.Next()).ToArray());
        }

        /// <summary>
        /// Gera um Value Object Password, já com hash interno via BCrypt.
        /// </summary>
        /// <param name="length">Tamanho mínimo da senha pura.</param>
        public static Password GeneratePasswordVO(int length = 8)
        {
            string raw = GenerateRawPassword(length);
            return Password.Create(raw);
        }
    }
}
