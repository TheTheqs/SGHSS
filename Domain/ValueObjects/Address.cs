// Domain/ValueObjects/Address.cs

using System.Text.RegularExpressions;

namespace SGHSS.Domain.ValueObjects
{
    /// <summary>
    /// Endereço canônico (BR). Campos explícitos com validação e normalização.
    /// Value: "Street, Number[ - Complement][ - District], City - State, CEP[', Country']".
    /// UF sempre em CAIXA ALTA e válida.
    /// CEP armazenado como 8 dígitos; ToString() formata NNNNN-NNN.
    /// Number aceita "S/N" (maiúsculo) ou dígitos/alfanumérico simples.
    /// </summary>
    public readonly record struct Address
    {
        private static readonly HashSet<string> UfSet = new(new[]
        {
            "AC","AL","AP","AM","BA","CE","DF","ES","GO","MA","MT","MS","MG",
            "PA","PB","PR","PE","PI","RJ","RN","RS","RO","RR","SC","SP","SE","TO"
        }, StringComparer.OrdinalIgnoreCase);

        public string Street { get; }
        public string Number { get; }
        public string? Complement { get; }
        public string? District { get; }
        public string City { get; }
        public string State { get; }
        public string Cep { get; }      // somente dígitos (8)
        public string Country { get; }  // default "BR"
        public string Value { get; }

        public Address(
            string street,
            string number,
            string city,
            string state,
            string cep,
            string? district = null,
            string? complement = null,
            string? country = "BR")
        {
            // Helpers
            static string Clean(string? s) => Regex.Replace((s ?? "").Trim(), @"\s+", " ");

            street = Clean(street);
            number = Clean(number);
            city = Clean(city);
            state = Clean(state).ToUpperInvariant();
            cep = Clean(cep);
            district = string.IsNullOrWhiteSpace(district) ? null : Clean(district);
            complement = string.IsNullOrWhiteSpace(complement) ? null : Clean(complement);
            country = string.IsNullOrWhiteSpace(country) ? "BR" : Clean(country);

            if (string.IsNullOrEmpty(street))
                throw new ArgumentException("Rua/Logradouro vazio.", nameof(street));
            if (string.IsNullOrEmpty(number))
                throw new ArgumentException("Número vazio.", nameof(number));
            if (string.IsNullOrEmpty(city))
                throw new ArgumentException("Cidade vazia.", nameof(city));
            if (!UfSet.Contains(state))
                throw new ArgumentException("UF inválida.", nameof(state));

            // Number: aceita "S/N" (qualquer variação) → normaliza para "S/N"
            if (Regex.IsMatch(number, @"^\s*s/?n\s*$", RegexOptions.IgnoreCase))
                number = "S/N";

            // CEP: guardar só dígitos; aceitar com/sem máscara
            var cepDigits = Regex.Replace(cep, @"\D", "");
            if (cepDigits.Length != 8)
                throw new ArgumentException("CEP deve conter 8 dígitos.", nameof(cep));

            // Opcional: recusar todos iguais em CEP (00000000)
            if (cepDigits.All(c => c == cepDigits[0]))
                throw new ArgumentException("CEP inválido (todos dígitos iguais).", nameof(cep));

            Street = street;
            Number = number;
            City = city;
            State = state;
            Cep = cepDigits;
            District = district;
            Complement = complement;
            Country = country!;

            // Montagem do Value
            string cepMasked = $"{Cep[..5]}-{Cep.Substring(5, 3)}";

            var partsMain = new List<string> { $"{Street}, {Number}" };
            if (!string.IsNullOrEmpty(Complement)) partsMain.Add($"{Complement}");

            string left = string.Join(" - ", partsMain);

            string middle = !string.IsNullOrEmpty(District)
                ? $"{left} - {District}"
                : left;

            string right = $"{City} - {State}, {cepMasked}";

            string countryPart = Country.Equals("BR", StringComparison.OrdinalIgnoreCase)
                ? ""
                : $", {Country}";

            Value = middle + ", " + right + countryPart;
        }

        public override string ToString() => Value;
    }
}
