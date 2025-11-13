// Domain/Models/Appointment.cs

using SGHSS.Domain.Enums;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Domain.Models
{
    public class Appointment
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public AppointmentType Type { get; set; }
        public Link Link { get; set; }
        public string Description { get; set; } = string.Empty;

        // Relacionamentos
        public ProfessionalSchedule ProfessionalSchedule { get; set; } = null!;
        public ScheduleSlot ScheduleSlot { get; set; } = null!;

        // Construtor padrão
        public Appointment() { }
    }
}
