namespace SGHSS.Domain.ValueObjects
{
    /// <summary>
    /// Representa um email imutável e validado.
    /// Normaliza para minúsculas e remove espaços em branco no início/fim.
    /// Bloqueia emails inválidos, com dois @ ou dois ".".
    /// </summary>
    public readonly record struct Email
    {
        public string Value { get; }
        public Email(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Email vazio.", nameof(input));
            var normalized = input.Trim().ToLowerInvariant();
            if (normalized.Any(char.IsWhiteSpace))
                throw new ArgumentException("Email inválido.", nameof(input));
            if (normalized.Count(c => c == '@') != 1 || normalized.Count(c => c == '.') != 1)
                throw new ArgumentException("Email inválido.", nameof(input));

            var atIndex = normalized.IndexOf('@');
            if (atIndex <= 0 || atIndex == normalized.Length - 1)
                throw new ArgumentException("Email inválido.", nameof(input));
            var dotIndex = normalized.IndexOf('.', atIndex);
            if (dotIndex <= atIndex + 1 || dotIndex == normalized.Length - 1)
                throw new ArgumentException("Email inválido.", nameof(input));
            Value = normalized;
        }
    }
}
