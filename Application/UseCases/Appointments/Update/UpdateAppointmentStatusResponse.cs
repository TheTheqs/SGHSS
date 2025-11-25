// Application/UseCases/Appointments/Update/UpdateAppointmentStatusResponse.cs

using SGHSS.Domain.Enums;

namespace SGHSS.Application.UseCases.Appointments.Update
{
    /// <summary>
    /// Representa o resultado da atualização de status de uma consulta
    /// e do respectivo slot de agenda.
    /// </summary>
    public sealed class UpdateAppointmentStatusResponse
    {
        /// <summary>
        /// Identificador da consulta cujo status foi atualizado.
        /// </summary>
        public Guid AppointmentId { get; }

        /// <summary>
        /// Identificador do slot de agenda associado à consulta.
        /// </summary>
        public Guid ScheduleSlotId { get; }

        /// <summary>
        /// Status final da consulta após a atualização.
        /// </summary>
        public AppointmentStatus AppointmentStatus { get; }

        /// <summary>
        /// Status final do slot de agenda após a atualização.
        /// </summary>
        public ScheduleSlotStatus ScheduleSlotStatus { get; }

        /// <summary>
        /// Cria uma nova instância de <see cref="UpdateAppointmentStatusResponse"/>
        /// com os metadados resultantes da operação.
        /// </summary>
        public UpdateAppointmentStatusResponse(
            Guid appointmentId,
            Guid scheduleSlotId,
            AppointmentStatus appointmentStatus,
            ScheduleSlotStatus scheduleSlotStatus)
        {
            AppointmentId = appointmentId;
            ScheduleSlotId = scheduleSlotId;
            AppointmentStatus = appointmentStatus;
            ScheduleSlotStatus = scheduleSlotStatus;
        }
    }
}
