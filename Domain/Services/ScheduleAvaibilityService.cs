// Domain/Services/ScheduleAvailabilityService.cs

using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;

namespace SGHSS.Domain.Services
{
    /// <summary>
    /// Serviço de domínio responsável por calcular a disponibilidade de horários
    /// de um profissional com base em uma SchedulePolicy e nos slots já existentes.
    /// </summary>
    /// <remarks>
    /// Este serviço gera intervalos potenciais de atendimento a partir das WeeklyWindows
    /// e da duração padrão definida na SchedulePolicy, removendo qualquer intervalo
    /// que conflite com slots já marcados na agenda.
    /// 
    /// Atenção: os valores de data e hora informados devem estar coerentes com o fuso
    /// horário definido em <see cref="SchedulePolicy.TimeZone"/>. A conversão entre UTC
    /// e o fuso apropriado deve ser feita externamente, na camada de aplicação.
    /// </remarks>
    public static class ScheduleAvailabilityService
    {
        /// <summary>
        /// Gera todos os intervalos de horário disponíveis para agendamento em um período,
        /// considerando a política de agendamento e os slots já existentes na agenda.
        /// </summary>
        /// <param name="policy">
        /// Política de agendamento que define duração dos atendimentos, fuso horário
        /// e janelas semanais recorrentes de disponibilidade.
        /// </param>
        /// <param name="existingSlots">
        /// Coleção de slots já existentes (ocupados, bloqueados, etc.) que devem ser
        /// considerados como indisponíveis no cálculo de novos horários.
        /// </param>
        /// <param name="from">
        /// Data e hora inicial (inclusiva) do intervalo de consulta de disponibilidade.
        /// </param>
        /// <param name="to">
        /// Data e hora final (exclusiva) do intervalo de consulta de disponibilidade.
        /// </param>
        /// <returns>
        /// Coleção de tuplas representando os intervalos disponíveis, onde cada item
        /// contém um <c>Start</c> e um <c>End</c> com o horário de início e fim do slot.
        /// </returns>
        public static IReadOnlyCollection<(DateTime Start, DateTime End)> GenerateAvailableIntervals(
            SchedulePolicy policy,
            IEnumerable<ScheduleSlot> existingSlots,
            DateTime from,
            DateTime to)
        {
            if (to <= from)
            {
                return Array.Empty<(DateTime Start, DateTime End)>();
            }

            int duration = policy.DurationInMinutes;
            if (duration <= 0)
            {
                throw new InvalidOperationException("A duração padrão dos atendimentos deve ser maior que zero.");
            }

            // Normaliza para uma lista para não enumerar várias vezes.
            var blockingSlots = existingSlots
                .Where(slot => slot.StartDateTime < to && slot.EndDateTime > from)
                .ToList();

            var result = new List<(DateTime Start, DateTime End)>();

            // Itera dia a dia dentro do intervalo solicitado.
            DateTime currentDate = from.Date;
            DateTime lastDate = to.Date;

            while (currentDate <= lastDate)
            {
                WeekDay dayOfWeek = MapSystemDayOfWeek(currentDate.DayOfWeek);

                // Todas as janelas semanais daquele dia (segunda, terça, etc.)
                var windowsForDay = policy.WeeklyWindows
                    .Where(w => w.DayOfWeek == dayOfWeek)
                    .ToList();

                foreach (var window in windowsForDay)
                {
                    // Constroi DateTime absolutos a partir da data do dia + TimeOnly da janela.
                    DateTime windowStart = currentDate.Add(window.StartTime.ToTimeSpan());
                    DateTime windowEnd = currentDate.Add(window.EndTime.ToTimeSpan());

                    // Garante que está dentro do intervalo [from, to)
                    if (windowEnd <= from || windowStart >= to)
                    {
                        continue;
                    }

                    // Começa a fatiar a janela na duração padrão
                    DateTime slotStart = windowStart;

                    while (true)
                    {
                        DateTime slotEnd = slotStart.AddMinutes(duration);

                        // Sai se ultrapassar o final da janela
                        if (slotEnd > windowEnd)
                        {
                            break;
                        }

                        // Respeita o intervalo global [from, to)
                        if (slotEnd <= from || slotStart >= to)
                        {
                            slotStart = slotStart.AddMinutes(duration);
                            continue;
                        }

                        bool hasConflict = blockingSlots.Any(slot =>
                            Overlaps(slotStart, slotEnd, slot.StartDateTime, slot.EndDateTime));

                        if (!hasConflict)
                        {
                            result.Add((slotStart, slotEnd));
                        }

                        slotStart = slotStart.AddMinutes(duration);
                    }
                }

                currentDate = currentDate.AddDays(1);
            }

            return result;
        }

        /// <summary>
        /// Verifica se dois intervalos de tempo se sobrepõem.
        /// </summary>
        private static bool Overlaps(
            DateTime start1,
            DateTime end1,
            DateTime start2,
            DateTime end2)
        {
            // Intervalos [start, end)
            return start1 < end2 && start2 < end1;
        }

        /// <summary>
        /// Mapeia o <see cref="System.DayOfWeek"/> para o enum de domínio <see cref="WeekDay"/>.
        /// </summary>
        private static WeekDay MapSystemDayOfWeek(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Sunday => WeekDay.Sunday,
                DayOfWeek.Monday => WeekDay.Monday,
                DayOfWeek.Tuesday => WeekDay.Tuesday,
                DayOfWeek.Wednesday => WeekDay.Wednesday,
                DayOfWeek.Thursday => WeekDay.Thursday,
                DayOfWeek.Friday => WeekDay.Friday,
                DayOfWeek.Saturday => WeekDay.Saturday,
                _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, null)
            };
        }
    }
}
