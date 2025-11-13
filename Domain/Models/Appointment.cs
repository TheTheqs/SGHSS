// Domain/Models/Appointment.cs

using SGHSS.Domain.Enums;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Domain.Models
{
    /// <summary>
    /// Representa uma consulta agendada, incluindo seu horário, status, tipo
    /// e demais informações relacionadas ao agendamento.
    /// </summary>
    /// <remarks>
    /// Uma consulta inclui detalhes como horário de início e término,
    /// status, tipo e entidades associadas, como a agenda do profissional
    /// e o slot específico utilizado. Utilize esta classe para modelar
    /// e gerenciar consultas individuais dentro do sistema de agendamentos.
    /// </remarks>

    public class Appointment
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public AppointmentType Type { get; set; }
        public Link? Link { get; set; }
        public string Description { get; set; } = string.Empty;

        // Relacionamentos
        public ProfessionalSchedule ProfessionalSchedule { get; set; } = null!;
        public ScheduleSlot ScheduleSlot { get; set; } = null!;

        // Construtor padrão
        public Appointment() { }
    }
}
