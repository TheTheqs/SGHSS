// Application/UseCases/Appointments/Register/ScheduleAppointmentRequest.cs

using SGHSS.Application.UseCases.Common;
using SGHSS.Domain.Enums;

namespace SGHSS.Application.UseCases.Appointments.Register
{
    /// <summary>
    /// Dados necessários para realizar o agendamento de uma consulta.
    /// </summary>
    public class ScheduleAppointmentRequest
    {
        /// <summary>
        /// Identificador do profissional que realizará a consulta.
        /// </summary>
        public Guid ProfessionalId { get; set; }

        /// <summary>
        /// Identificador do paciente que será atendido.
        /// </summary>
        public Guid PatientId { get; set; }

        /// <summary>
        /// Slot de agenda desejado para o agendamento.
        /// </summary>
        public ScheduleSlotDto Slot { get; set; } = null!;

        /// <summary>
        /// Tipo da consulta a ser agendada.
        /// </summary>
        public AppointmentType Type { get; set; }

        /// <summary>
        /// Descrição opcional do agendamento.
        /// </summary>
        public string? Description { get; set; }
    }
}
