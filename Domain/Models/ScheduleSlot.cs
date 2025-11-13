// Domain/Models/ScheduleSlot.cs

using SGHSS.Domain.Enums;

namespace SGHSS.Domain.Models
{
    public class ScheduleSlot
    {
        public Guid Id { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public ScheduleSlotStatus Status { get; set; }

        // Relacionamentos
        public ProfessionalSchedule ProfessionalSchedule { get; set; } = null!;
        public Appointment? Appointment { get; set; }

        // Construtor padrão
        public ScheduleSlot() { }
    }
}
