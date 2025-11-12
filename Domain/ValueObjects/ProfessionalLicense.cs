// Domain/ValueObjects/ProfessionalLicense.cs

using System.Text.RegularExpressions;

namespace SGHSS.Domain.ValueObjects
{
    /// <summary>
    /// Registro profissional (ex.: CRM-SP 123456).
    /// Aceita múltiplas variações de entrada e separadores.
    /// Normaliza para o formato canônico: "COUNCIL-UF NUMBER" (ex.: "CRM-SP 123456").
    /// Council/UF em caixa alta; Number somente dígitos (preserva zeros à esquerda).
    /// Duplicadas de qualquer espécie devem recusar a instânciação (UF, CONCIL ou NUMBER não podem estar duplicados na entrada).
    /// Whitelist de conselhos profissionais suportados (ajustável conforme necessário). API externa para verificação é desejável futuramente.
    /// </summary>
    public readonly record struct ProfessionalLicense
    {
        // Conselhos profissionais suportados. Deve ser ajustado conforme necessário.
        private static readonly string[] CouncilsWhitelist =
        [
            "CRM",   // Medicina
            "COREN", // Enfermagem
            "CRP",   // Psicologia
            "CRF",   // Farmácia
            "CRO",   // Odontologia
            "CREFITO", // Fisioterapia/Terapia Ocupacional
            "CRN",   // Nutrição
            "CREFONO" // Fonoaudiologia
        ];

        // UFs do Brasil
        private static readonly string[] UFs =
        [
            "AC","AL","AP","AM","BA","CE","DF","ES","GO","MA","MT","MS","MG",
            "PA","PB","PR","PE","PI","RJ","RN","RS","RO","RR","SC","SP","SE","TO"
        ];

        private static readonly HashSet<string> CouncilSet = new(CouncilsWhitelist, StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> UfSet = new(UFs, StringComparer.OrdinalIgnoreCase);

        public string Council { get; }
        public string State { get; }
        public string Number { get; }
        public string Value { get; }

        public ProfessionalLicense(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Registro profissional vazio.", nameof(input));

            input = input.Trim();

            // Normaliza separadores comuns para espaço
            var normalized = Regex.Replace(input, @"[./\\\-]+", " ");
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

            // Tenta extrair as partes em qualquer ordem.
            string? council = null;
            string? uf = null;
            string? number = null;


            foreach (var rawToken in normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var token = rawToken.Trim();

                if (token.Length == 0) continue;

                // 1) Dígitos puros => número
                if (Regex.IsMatch(token, @"^\d+$"))
                {
                    if (number is not null)
                        throw new ArgumentException("Registro profissional inválido: múltiplos números.", nameof(input));

                    number = token;
                    continue;
                }

                // 2) Letras puras => pode ser UF ou Conselho
                if (Regex.IsMatch(token, @"^[A-Za-z]+$"))
                {
                    if (token.Length == 2 && UfSet.Contains(token))
                    {
                        if (uf is not null)
                            throw new ArgumentException("Registro profissional inválido: múltiplas UFs.", nameof(input));

                        uf = token.ToUpperInvariant();
                        continue;
                    }

                    if (CouncilSet.Contains(token))
                    {
                        if (council is not null)
                            throw new ArgumentException("Registro profissional inválido: múltiplos conselhos.", nameof(input));

                        council = token.ToUpperInvariant();
                        continue;
                    }
                }

                // 3) Misto: UF + Número (ex.: "SP123456")
                var m1 = Regex.Match(token, @"^([A-Za-z]{2})(\d{2,})$");
                if (m1.Success && UfSet.Contains(m1.Groups[1].Value))
                {
                    if (uf is not null)
                        throw new ArgumentException("Registro profissional inválido: múltiplas UFs.", nameof(input));
                    if (number is not null)
                        throw new ArgumentException("Registro profissional inválido: múltiplos números.", nameof(input));

                    uf = m1.Groups[1].Value.ToUpperInvariant();
                    number = m1.Groups[2].Value;
                    continue;
                }

                // ou Número + UF (ex.: "123456SP")
                var m2 = Regex.Match(token, @"^(\d{2,})([A-Za-z]{2})$");
                if (m2.Success && UfSet.Contains(m2.Groups[2].Value))
                {
                    if (number is not null)
                        throw new ArgumentException("Registro profissional inválido: múltiplos números.", nameof(input));
                    if (uf is not null)
                        throw new ArgumentException("Registro profissional inválido: múltiplas UFs.", nameof(input));

                    number = m2.Groups[1].Value;
                    uf = m2.Groups[2].Value.ToUpperInvariant();
                    continue;
                }

            }

            // Valida presença das três partes
            if (council is null || uf is null || number is null)
            {
                // Tentativa extra: alguns formatos podem ter "CRM SP123456" (já coberto) ou "CRM-SP123456" (já coberto)
                // Se ainda assim não preencheu, é inválido.
                throw new ArgumentException("Registro profissional inválido: informe Conselho, UF e Número.", nameof(input));
            }

            // Valida conselho
            if (!CouncilSet.Contains(council))
                throw new ArgumentException("Conselho profissional inválido ou não suportado.", nameof(input));

            // Valida UF
            if (!UfSet.Contains(uf))
                throw new ArgumentException("UF inválida.", nameof(input));

            // Limpa e valida número (somente dígitos; preserva zeros à esquerda)
            if (!Regex.IsMatch(number, @"^\d+$"))
                throw new ArgumentException("Número do registro deve conter apenas dígitos.", nameof(input));

            // Tamanho do número: ajuste conforme a política.
            if (number.Length < 4 || number.Length > 7)
                throw new ArgumentException("Número do registro deve conter entre 4 e 7 dígitos.", nameof(input));

            Council = council.ToUpperInvariant();
            State = uf.ToUpperInvariant();
            Number = number;

            Value = $"{Council}-{State} {Number}";
        }

        public override string ToString() => Value;
    }
}
