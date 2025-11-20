// Tests/TestData/ValueObjects/AddressGenerator.cs

using SGHSS.Application.UseCases.Common;

namespace SGHSS.Tests.TestData.ValueObjects
{

    /// <summary>
    /// Classe utilitária para geração de endereços brasileiros válidos,
    /// compatíveis com o VO Address.
    /// </summary>
    public static class AddressGenerator
    {
        private static readonly Random Rng = new();

        // Ruas/Logradouros comuns
        private static readonly List<string> Streets = new()
    {
        "Rua das Flores",
        "Avenida Brasil",
        "Rua XV de Novembro",
        "Rua São João",
        "Rua das Palmeiras",
        "Avenida Paulista",
        "Rua Dom Pedro II",
        "Rua Rio Branco",
        "Rua Sete de Setembro",
        "Rua Mato Grosso",
        "Avenida Independência",
        "Rua dos Bandeirantes"
    };

        // Bairros
        private static readonly List<string> Districts = new()
    {
        "Centro",
        "Jardim América",
        "Vila Nova",
        "Bela Vista",
        "Santa Cecília",
        "Boa Vista",
        "São José",
        "Jardim das Acácias",
        "Vila Mariana",
        "Jardim Europa"
    };

        // Cidades
        private static readonly List<string> Cities = new()
    {
        "São Paulo",
        "Rio de Janeiro",
        "Belo Horizonte",
        "Curitiba",
        "Porto Alegre",
        "Salvador",
        "Fortaleza",
        "Recife",
        "Florianópolis",
        "Campinas",
        "Goiânia"
    };

        // UFs válidas (batendo com o VO)
        private static readonly List<string> States = new()
    {
        "AC","AL","AP","AM","BA","CE","DF","ES","GO","MA","MT","MS","MG",
        "PA","PB","PR","PE","PI","RJ","RN","RS","RO","RR","SC","SP","SE","TO"
    };

        // Complementos opcionais
        private static readonly List<string> Complements = new()
    {
        "Apto 101",
        "Apto 202",
        "Bloco A",
        "Bloco B",
        "Casa 1",
        "Casa 2",
        "Fundos",
        "Sobrado",
        "Loja 3",
        "Sala 05"
    };

        /// <summary>
        /// Gera um Address válido com dados brasileiros fictícios.
        /// </summary>
        /// <returns>Instância válida de Address.</returns>
        public static AddressDto GenerateAddress()
        {
            string street = PickRandom(Streets);
            string city = PickRandom(Cities);
            string state = PickRandom(States);

            string number = GenerateNumber();
            string cep = GenerateCep();

            // Gera distrito e complemento de forma opcional
            string? district = Rng.NextDouble() < 0.8 ? PickRandom(Districts) : null;
            string? complement = Rng.NextDouble() < 0.5 ? PickRandom(Complements) : null;

            // Country deixamos como null/BR para seguir o default do DTO
            return new AddressDto
                {
                    Street = street,
                    Number =  number,
                    City = city,
                    State = state,
                    Cep = cep,
                    District = district,
                    Complement = complement,
                    Country = "BR"
                };
        }

        /// <summary>
        /// Gera um CEP válido (8 dígitos, não todos iguais).
        /// </summary>
        private static string GenerateCep()
        {
            while (true)
            {
                int[] digits = new int[8];
                for (int i = 0; i < 8; i++)
                    digits[i] = Rng.Next(0, 10);

                // Evita CEP com todos os dígitos iguais (rejeitado pelo VO)
                if (!digits.All(d => d == digits[0]))
                    return string.Concat(digits.Select(d => d.ToString()));
            }
        }

        /// <summary>
        /// Gera um número de endereço:
        /// - Pode ser "S/N"
        /// - Ou um número simples, com possível sufixo alfanumérico (ex.: 123, 45A).
        /// </summary>
        private static string GenerateNumber()
        {
            // Pequena chance de "S/N"
            if (Rng.NextDouble() < 0.1)
                return "S/N";

            int baseNumber = Rng.Next(1, 9999);

            // Às vezes adiciona letra (A, B, C...)
            if (Rng.NextDouble() < 0.3)
            {
                char suffix = (char)('A' + Rng.Next(0, 3)); // A, B ou C
                return $"{baseNumber}{suffix}";
            }

            return baseNumber.ToString();
        }

        /// <summary>
        /// Helper para pegar um elemento aleatório de uma lista.
        /// </summary>
        private static string PickRandom(List<string> list) =>
            list[Rng.Next(list.Count)];
    }

}
