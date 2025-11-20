// Tests/TestData/ValueObjects/NameGenerator.cs

namespace SGHSS.Tests.TestData.ValueObjects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SGHSS.Domain.Enums;

    /// <summary>
    /// Gera nomes completos válidos para uso em testes e permite inferir o sexo
    /// a partir do primeiro nome.
    /// </summary>
    public static class NameGenerator
    {
        private static readonly Random Rng = new();

        // ===== Nomes masculinos =====
        private static readonly List<string> MaleFirstNames = new()
    {
        "João", "Pedro", "Lucas", "Marcos", "Gabriel", "Paulo", "Ricardo",
        "Mateus", "Miguel", "Arthur", "Heitor", "Felipe", "Gustavo", "Bruno",
        "Diego", "Samuel", "Caio", "Leonardo"
    };

        // ===== Nomes femininos =====
        private static readonly List<string> FemaleFirstNames = new()
    {
        "Ana", "Maria", "Larissa", "Fernanda", "Juliana", "Amanda", "Carolina",
        "Beatriz", "Sofia", "Helena", "Mariana", "Camila", "Patrícia", "Letícia",
        "Aline", "Bianca", "Renata", "Daniela"
    };

        // Sobrenomes
        private static readonly List<string> MiddleNames = new()
    {
        "Almeida", "Silva", "Oliveira", "Souza", "Lima", "Cardoso", "Pereira",
        "Araujo", "Melo", "Costa", "Gomes", "Teixeira", "Barbosa", "Rocha"
    };

        private static readonly List<string> LastNames = new()
    {
        "da Silva", "dos Santos", "do Carmo", "de Souza", "de Lima", "de Oliveira",
        "da Costa", "dos Reis", "de Andrade", "da Rocha", "de Carvalho"
    };

        /// <summary>
        /// Retorna um nome completo gerado aleatoriamente.
        /// </summary>
        public static string GetFullName()
        {
            // 50% masculino, 50% feminino
            bool isMale = Rng.NextDouble() < 0.5;

            string first = isMale
                ? MaleFirstNames[Rng.Next(MaleFirstNames.Count)]
                : FemaleFirstNames[Rng.Next(FemaleFirstNames.Count)];

            string middle = MiddleNames[Rng.Next(MiddleNames.Count)];
            string last = LastNames[Rng.Next(LastNames.Count)];

            return $"{first} {middle} {last}";
        }

        /// <summary>
        /// Infere o sexo a partir do primeiro nome.
        /// Retorna Male, Female ou Other se não encontrado.
        /// </summary>
        public static Sex InferSexFromName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return Sex.Other;

            // pega somente o primeiro token
            string first = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];

            if (MaleFirstNames.Contains(first, StringComparer.OrdinalIgnoreCase))
                return Sex.Male;

            if (FemaleFirstNames.Contains(first, StringComparer.OrdinalIgnoreCase))
                return Sex.Female;

            return Sex.Other;
        }
    }

}
