// Tests/TestData/Models/SchedulePolicyGenerator.cs

using SGHSS.Application.UseCases.Common;
using SGHSS.Tests.TestData.Models;

namespace SGHSS.Tests.TestData.Common
{
    /// <summary>
    /// Classe utilitária para geração de instâncias válidas de <see cref="SchedulePolicyDto"/>,
    /// simulando políticas de agendamento reais para uso em testes.
    /// </summary>
    public static class SchedulePolicyGenerator
    {
        private static readonly Random Rng = new();

        // Fusos horários válidos segundo o VO TimeZone
        private static readonly List<string> ValidTimeZones = new()
        {
            "Etc/UTC",
            "America/Sao_Paulo",
            "America/New_York",
            "Europe/London",
            "UTC-03:00",
            "UTC+01:00"
        };

        /// <summary>
        /// Gera uma política de agendamento com duração e quantidade de janelas configuráveis.
        /// </summary>
        /// <param name="durationInMinutes">
        /// Duração padrão dos atendimentos, em minutos. Caso seja <c>null</c>, utiliza 30 minutos.
        /// </param>
        /// <param name="windowsCount">
        /// Quantidade de janelas semanais a serem geradas. O valor padrão é 2.
        /// </param>
        /// <returns>
        /// Uma instância de <see cref="SchedulePolicyDto"/> com dados consistentes.
        /// </returns>
        public static SchedulePolicyDto GeneratePolicy(int? durationInMinutes = null, int windowsCount = 2)
        {
            if (windowsCount < 1)
                windowsCount = 1;

            int effectiveDuration = durationInMinutes ?? 30;

            var policy = new SchedulePolicyDto
            {
                DurationInMinutes = effectiveDuration,
                TimeZone = PickRandomTimeZone(),
                WeeklyWindows = new List<WeeklyWindowDto>()
            };

            for (int i = 0; i < windowsCount; i++)
            {
                var window = WeeklyWindowGenerator.GenerateWeeklyWindow();
                policy.WeeklyWindows.Add(window);
            }

            return policy;
        }

        /// <summary>
        /// Seleciona um fuso horário válido aleatoriamente.
        /// </summary>
        private static string PickRandomTimeZone()
        {
            return ValidTimeZones[Rng.Next(ValidTimeZones.Count)];
        }
    }
}
