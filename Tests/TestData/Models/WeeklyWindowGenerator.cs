// Tests/TestData/Models/WeeklyWindowGenerator.cs

using SGHSS.Application.UseCases.Common;
using SGHSS.Domain.Enums;

namespace SGHSS.Tests.TestData.Models
{
    /// <summary>
    /// Classe utilitária para gerar instâncias válidas de <see cref="WeeklyWindowDto"/>,
    /// simulando horários reais de disponibilidade semanal.
    /// </summary>
    public static class WeeklyWindowGenerator
    {
        private static readonly Random Rng = new();

        /// <summary>
        /// Gera uma janela semanal coerente, contendo:
        /// - Dia da semana aleatório
        /// - Horário inicial realista (entre 06h e 15h)
        /// - Horário final posterior ao início (entre +1h e +8h)
        /// </summary>
        public static WeeklyWindowDto GenerateWeeklyWindow()
        {
            var day = PickRandomDay();
            var (start, end) = GenerateRealisticTimeSpan();

            return new WeeklyWindowDto
            {
                DayOfWeek = day,
                StartTime = start,
                EndTime = end
            };
        }

        /// <summary>
        /// Seleciona um dia da semana aleatoriamente.
        /// </summary>
        private static WeekDay PickRandomDay()
        {
            var values = Enum.GetValues<WeekDay>();
            return values[Rng.Next(values.Length)];
        }

        /// <summary>
        /// Gera um intervalo realista de horário:
        /// - Início entre 06h00 e 15h00
        /// - Duração entre 1 e 8 horas
        /// </summary>
        private static (TimeOnly start, TimeOnly end) GenerateRealisticTimeSpan()
        {
            int startHour = Rng.Next(6, 16); // entre 06:00 e 15:00
            int startMinute = Rng.Next(0, 60);

            var start = new TimeOnly(startHour, startMinute);

            int durationHours = Rng.Next(1, 9); // 1 a 8 horas
            var end = start.AddHours(durationHours);

            // Limite do dia (não costuma atender após 22h)
            if (end.Hour > 22)
            {
                end = new TimeOnly(22, 0);
            }

            return (start, end);
        }
    }
}
