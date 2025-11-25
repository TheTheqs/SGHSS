// Application/UseCases/Appointments/Register/ScheduleAppointmentResponse.cs

namespace SGHSS.Application.UseCases.Appointments.Register
{
    /// <summary>
    /// Resultado do agendamento de uma consulta.
    /// </summary>
    public class ScheduleAppointmentResponse
    {
        public Guid AppointmentId { get; set; }
        public Guid ScheduleSlotId { get; set; }
        public string? Link { get; set; } = string.Empty;
    }
}
