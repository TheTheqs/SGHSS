// Domain/Models/SchedulePolicy.cs

using TimeZone = SGHSS.Domain.ValueObjects.TimeZone; // Evita conflito com System.TimeZone

namespace SGHSS.Domain.Models
{
    public class SchedulePolicy
    {
        public Guid Id { get; set; }
        public int DurationInMinutes { get; set; }
        public TimeZone TimeZone { get; set; }

        // Relacionamentos
        public ProfessionalSchedule ProfessionalSchedule { get; set; } = null!;
        public ICollection<WeeklyWindow> WeeklyWindows { get; set; } = new List<WeeklyWindow>();

        // Construtor padrão
        public SchedulePolicy() { }
    }
}
