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
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public AppointmentStatus Status { get; set; } // Enum
        public AppointmentType Type { get; set; } // Enum
        public Link? Link { get; set; } // Value Object
        public string Description { get; set; } = string.Empty;

        // Relacionamentos
        public ScheduleSlot ScheduleSlot { get; set; } = null!;
        public MedicalRecordUpdate? MedicalRecordUpdate { get; set; }
        public EletronicPrescription? EletronicPrescription { get; set; }
        public DigitalMedicalCertificate? DigitalMedicalCertificate { get; set; }

        // Construtor padrão
        public Appointment() { }
    }
}
