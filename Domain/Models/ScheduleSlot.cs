// Domain/Models/ScheduleSlot.cs

using SGHSS.Domain.Enums;

namespace SGHSS.Domain.Models
{
    /// <summary>
    /// Representa um intervalo de tempo dentro da agenda de um profissional, incluindo seu horário,
    /// status e informações relacionadas a uma possível consulta.
    /// </summary>
    /// <remarks>
    /// Um slot de agenda define um período específico que pode estar disponível, reservado ou marcado
    /// de acordo com seu status. Cada slot está associado à agenda de um profissional e pode ser,
    /// opcionalmente, vinculado a uma consulta. Esta classe é utilizada para gerenciar e consultar
    /// disponibilidade ou reservas dentro dos sistemas de agendamento.
    /// </remarks>
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
