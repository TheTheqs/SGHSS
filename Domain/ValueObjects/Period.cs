// Domain/ValueObjects/Period.cs

namespace SGHSS.Domain.ValueObjects
{
    /// <summary>
    /// Período de datas (intervalo fechado e inclusivo) sem horário.
    /// Campos: Start (início), End (fim), ambos DateOnly.
    /// Invariante: Start <= End.
    /// Value canônico: "yyyy-MM-dd..yyyy-MM-dd".
    /// Fábrica: ForMonth(ano, mês) → 1º ao último dia do mês.
    /// ToString() retorna o Value.
    /// </summary>
    public readonly record struct Period
    {
        public DateOnly Start { get; }
        public DateOnly End { get; }
        public string Value { get; }

        public Period(DateOnly start, DateOnly end)
        {
            if (start > end)
                throw new ArgumentException("Perído inválido: começo deve vir antes do fim.", nameof(start));

            Start = start;
            End = end;
            Value = $"{Start:yyyy-MM-dd}..{End:yyyy-MM-dd}";
        }

        // Cria um período correspondente ao mês/ano informado (1º ao último dia).
        public static Period ForMonth(int year, int month)
        {
            if (month < 1 || month > 12)
                throw new ArgumentException("Invalid month (1-12).", nameof(month));

            var start = new DateOnly(year, month, 1);
            var end = start.AddMonths(1).AddDays(-1);
            return new Period(start, end);
        }

        public override string ToString() => Value;
    }
}
