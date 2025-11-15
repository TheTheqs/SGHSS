// Domain/Models/WeeklyWindow.cs

using SGHSS.Domain.Enums;
namespace SGHSS.Domain.Models

{
    /// <summary>
    /// Representa uma janela de tempo semanal recorrente em um dia específico, com um intervalo de início e término,
    /// normalmente usada para definir períodos de atividade ou disponibilidade dentro de uma agenda.
    /// </summary>
    /// <remarks>
    /// Uma WeeklyWindow especifica um dia da semana e um horário inicial e final, permitindo que aplicações
    /// modelem intervalos recorrentes, como horários de atendimento, janelas de manutenção ou períodos de acesso.
    /// A janela está associada a uma SchedulePolicy, que pode definir regras ou restrições adicionais para o
    /// agendamento dentro desse período.
    /// </remarks>
    public class WeeklyWindow
    {
        public Guid Id { get; set; }
        public WeekDay DayOfWeek { get; set; } // Enum
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        // Relacionamentos
        public SchedulePolicy SchedulePolicy { get; set; } = null!;

        // Construtor padrão
        public WeeklyWindow() { }
    }
}
