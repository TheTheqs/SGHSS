// Application/UseCases/Appointments/Update/UpdateAppointmentStatusRequest.cs

using SGHSS.Domain.Enums;

namespace SGHSS.Application.UseCases.Appointments.Update
{
    /// <summary>
    /// Representa os dados necessários para atualizar o status
    /// de uma consulta (appointment) e do respectivo slot de agenda.
    /// </summary>
    /// <remarks>
    /// Este request é utilizado em cenários como:
    /// cancelamento de consulta, ausência do paciente, recusa ou
    /// qualquer outra transição de estado permitida pelas regras
    /// de negócio entre <see cref="AppointmentStatus"/> e
    /// <see cref="ScheduleSlotStatus"/>.
    /// </remarks>
    public sealed class UpdateAppointmentStatusRequest
    {
        /// <summary>
        /// Identificador da consulta cujo status deverá ser atualizado.
        /// </summary>
        public Guid AppointmentId { get; init; }

        /// <summary>
        /// Novo status a ser aplicado à consulta.
        /// </summary>
        public AppointmentStatus AppointmentStatus { get; init; }

        /// <summary>
        /// Novo status a ser aplicado ao slot de agenda associado à consulta.
        /// </summary>
        public ScheduleSlotStatus ScheduleSlotStatus { get; init; }
    }
}
