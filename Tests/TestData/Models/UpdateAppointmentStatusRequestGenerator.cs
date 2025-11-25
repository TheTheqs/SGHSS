// Tests/TestData/Models/UpdateAppointmentStatusRequestGenerator.cs

using SGHSS.Application.UseCases.Appointments.Update;
using SGHSS.Domain.Enums;

namespace SGHSS.Tests.TestData.Appointments
{
    /// <summary>
    /// Classe utilitária para geração de instâncias de
    /// <see cref="UpdateAppointmentStatusRequest"/> com valores fictícios,
    /// porém válidos, para uso em testes.
    /// </summary>
    public static class UpdateAppointmentStatusRequestGenerator
    {
        /// <summary>
        /// Gera um request completo para atualização de status de consulta
        /// e slot de agenda, permitindo sobrescrita de campos específicos.
        /// </summary>
        /// <param name="providedAppointmentId">
        /// Identificador da consulta a ser utilizada. Caso seja <c>null</c>,
        /// um novo <see cref="Guid"/> será gerado.
        /// </param>
        /// <param name="providedAppointmentStatus">
        /// Status desejado para a consulta. Caso não informado, será utilizado
        /// <see cref="AppointmentStatus.Canceled"/>.
        /// </param>
        /// <param name="providedScheduleSlotStatus">
        /// Status desejado para o slot de agenda. Caso não informado, será utilizado
        /// <see cref="ScheduleSlotStatus.Completed"/>.
        /// </param>
        /// <returns>
        /// Instância de <see cref="UpdateAppointmentStatusRequest"/> pronta
        /// para uso em cenários de teste.
        /// </returns>
        public static UpdateAppointmentStatusRequest Generate(
            Guid? providedAppointmentId = null,
            AppointmentStatus? providedAppointmentStatus = null,
            ScheduleSlotStatus? providedScheduleSlotStatus = null
        )
        {
            Guid appointmentId = providedAppointmentId ?? Guid.NewGuid();
            AppointmentStatus appointmentStatus = providedAppointmentStatus ?? AppointmentStatus.Canceled;
            ScheduleSlotStatus slotStatus = providedScheduleSlotStatus ?? ScheduleSlotStatus.Completed;

            return new UpdateAppointmentStatusRequest
            {
                AppointmentId = appointmentId,
                AppointmentStatus = appointmentStatus,
                ScheduleSlotStatus = slotStatus
            };
        }
    }
}
