// Domain/Models/WeeklyWindow.cs

using SGHSS.Domain.Enums;
namespace SGHSS.Domain.Models

{
    public class WeeklyWindow
    {
        public Guid Id { get; set; }
        public WeekDay DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        // Relacionamentos
        public SchedulePolicy SchedulePolicy { get; set; } = null!;

        // Construtor padrão
        public WeeklyWindow() { }
    }
}
