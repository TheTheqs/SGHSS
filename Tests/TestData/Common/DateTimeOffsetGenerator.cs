// Tests/TestData/Common/DateTimeOffsetGenerator.cs


namespace SGHSS.Tests.TestData.Common
{

    /// <summary>
    /// Classe utilitária para geração de datas aleatórias usando DateTimeOffset.
    /// </summary>
    public static class DateTimeOffsetGenerator
    {
        private static readonly Random Rng = new();

        /// <summary>
        /// Gera uma data de nascimento válida para alguém com 18 anos ou mais.
        /// Faixa típica: entre 18 e 80 anos atrás.
        /// </summary>
        public static DateTimeOffset GenerateBirthDate()
        {
            int minAge = 18;
            int maxAge = 80;

            int yearsAgo = Rng.Next(minAge, maxAge + 1);
            int daysOffset = Rng.Next(0, 365);

            return DateTimeOffset.UtcNow
                .AddYears(-yearsAgo)
                .AddDays(-daysOffset);
        }

        /// <summary>
        /// Gera uma data recente (ex.: últimos 30 dias).
        /// </summary>
        public static DateTimeOffset GenerateRecentDate(int daysRange = 30)
        {
            int days = Rng.Next(1, daysRange + 1);
            int seconds = Rng.Next(0, 86400); // aleatoriedade dentro do dia

            return DateTimeOffset.UtcNow
                .AddDays(-days)
                .AddSeconds(-seconds);
        }

        /// <summary>
        /// Gera uma data anterior à referência, com limite de até 3 meses (90 dias).
        /// </summary>
        public static DateTimeOffset GenerateBefore(DateTimeOffset reference, int maxDays = 90)
        {
            int days = Rng.Next(1, maxDays + 1);
            int seconds = Rng.Next(0, 86400);

            return reference
                .AddDays(-days)
                .AddSeconds(-seconds);
        }

        /// <summary>
        /// Gera uma data posterior à referência, com limite de até 3 meses (90 dias).
        /// </summary>
        public static DateTimeOffset GenerateAfter(DateTimeOffset reference, int maxDays = 90)
        {
            int days = Rng.Next(1, maxDays + 1);
            int seconds = Rng.Next(0, 86400);

            return reference
                .AddDays(days)
                .AddSeconds(seconds);
        }
    }

}
