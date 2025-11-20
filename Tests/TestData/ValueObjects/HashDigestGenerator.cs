// Tests/TestData/ValueObjects/HashDigestGenerator.cs

using System;
using System.Linq;
using System.Text;

namespace SGHSS.Tests.TestData.ValueObjects
{
    /// <summary>
    /// Classe utilitária para geração de Hash Digests hexadecimais válidos,
    /// de tamanhos compatíveis com o VO HashDigest.
    /// </summary>
    public static class HashDigestGenerator
    {
        private static readonly Random Rng = new();

        // Tamanhos suportados (em caracteres hex)
        private static readonly int[] AllowedLengths = { 32, 40, 56, 64, 96, 128 };

        private const string HexChars = "0123456789abcdef";

        /// <summary>
        /// Gera um hash hexadecimal válido, com um dos tamanhos suportados pelo sistema.
        /// A saída já vem normalizada em minúsculas e sem separadores.
        /// </summary>
        /// <returns>Uma string contendo o hash hexadecimal.</returns>
        public static string GenerateHash()
        {
            // Escolhe um tamanho permitido
            int length = AllowedLengths[Rng.Next(AllowedLengths.Length)];

            var sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                sb.Append(HexChars[Rng.Next(16)]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gera um HashDigest pronto para uso, já validado pelo VO.
        /// </summary>
        /// <returns>Instância válida de HashDigest.</returns>
        public static SGHSS.Domain.ValueObjects.HashDigest GenerateHashDigest()
        {
            var hex = GenerateHash();
            return new SGHSS.Domain.ValueObjects.HashDigest(hex);
        }
    }

}
