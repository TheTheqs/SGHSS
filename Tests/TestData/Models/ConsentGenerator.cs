// Tests/TestData/Models/ConsentGenerator.cs

using SGHSS.Application.UseCases.Common;
using SGHSS.Domain.Enums;
using SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.ValueObjects;

namespace SGHSS.Tests.TestData.Models
{

    namespace SGHSS.Tests.TestData.Common
    {
        /// <summary>
        /// Classe utilitária para geração de objetos ConsentDto válidos
        /// para uso em testes, com datas e hash gerados automaticamente.
        /// </summary>
        public static class ConsentGenerator
        {
            private static readonly Random Rng = new();

            /// <summary>
            /// Gera um ConsentDto com base no escopo, canal e status de validade.
            /// </summary>
            /// <param name="scope">Escopo do consentimento.</param>
            /// <param name="channel">Canal por meio do qual o consentimento foi fornecido.</param>
            /// <param name="isValid">
            /// Indica se o consentimento está válido (não revogado).
            /// true  → RevocationDate = null, ConsentDate recente.
            /// false → RevocationDate recente, ConsentDate anterior (até 3 meses antes).
            /// </param>
            /// <returns>Um ConsentDto preenchido com dados consistentes.</returns>
            public static ConsentDto GenerateConsent(ConsentScope scope, ConsentChannel channel, bool isValid)
            {
                // Versão dos termos (ex.: v1.0, v2.3...)
                string termVersion = GenerateTermVersion();

                // Hash dos termos (hex válido conforme VO HashDigest)
                string termHash = HashDigestGenerator.GenerateHash();

                DateTimeOffset consentDate;
                DateTimeOffset? revocationDate = null;

                if (isValid)
                {
                    // Consentimento ainda válido: data recente, sem revogação
                    consentDate = DateTimeOffsetGenerator.GenerateRecentDate();
                    revocationDate = null;
                }
                else
                {
                    // Consentimento revogado:
                    // 1) Gera uma revogação recente
                    revocationDate = DateTimeOffsetGenerator.GenerateRecentDate();

                    // 2) Gera o consentimento antes dessa revogação (até 3 meses antes)
                    consentDate = DateTimeOffsetGenerator.GenerateBefore(revocationDate.Value);
                }

                return new ConsentDto
                {
                    Scope = scope,
                    TermVersion = termVersion,
                    Channel = channel,
                    ConsentDate = consentDate,
                    RevocationDate = revocationDate,
                    TermHash = termHash
                };
            }

            /// <summary>
            /// Gera uma versão de termo simples no formato "vX.Y".
            /// </summary>
            private static string GenerateTermVersion()
            {
                int major = Rng.Next(1, 4); // 1 a 3
                int minor = Rng.Next(0, 6); // 0 a 5
                return $"v{major}.{minor}";
            }
        }
    }

}
