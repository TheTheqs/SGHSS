// Application/UseCases/Appointments/Update/UpdateAppointmentStatusUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Enums;

namespace SGHSS.Application.UseCases.Appointments.Update
{
    /// <summary>
    /// Caso de uso responsável por atualizar o status de uma consulta
    /// (appointment) e do respectivo slot de agenda associado.
    /// </summary>
    /// <remarks>
    /// Este caso de uso é indicado para cenários como cancelamento de consultas,
    /// marcação de ausência do paciente, recusa de atendimento ou outras
    /// transições válidas entre <see cref="AppointmentStatus"/> e
    /// <see cref="ScheduleSlotStatus"/>.
    /// </remarks>
    public sealed class UpdateAppointmentStatusUseCase
    {
        private readonly IAppointmentRepository _appointmentRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso de atualização de status de consulta.
        /// </summary>
        /// <param name="appointmentRepository">Repositório de consultas (appointments).</param>
        public UpdateAppointmentStatusUseCase(IAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        /// <summary>
        /// Executa a atualização de status da consulta e do slot de agenda associado.
        /// </summary>
        /// <param name="request">
        /// Dados necessários para localizar a consulta e aplicar os novos status.
        /// </param>
        /// <returns>
        /// Um <see cref="UpdateAppointmentStatusResponse"/> contendo os metadados
        /// da consulta e do slot após a atualização.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Lançada quando o <paramref name="request"/> é nulo.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando a consulta não é encontrada, não possui slot associado
        /// ou quando seu status atual não permite alteração.
        /// </exception>
        public async Task<UpdateAppointmentStatusResponse> Handle(UpdateAppointmentStatusRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            // 1) Carrega a consulta com o slot associado
            var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId);

            if (appointment is null)
                throw new InvalidOperationException("Consulta informada não foi encontrada.");

            if (appointment.ScheduleSlot is null)
                throw new InvalidOperationException("A consulta informada não possui slot de agenda associado.");

            // 2) Regra de negócio: consultas finalizadas ou canceladas não podem ser alteradas
            if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Canceled)
            {
                throw new InvalidOperationException(
                    "Consultas concluídas ou canceladas não podem ter o status alterado.");
            }

            // 3) Aplica os novos status
            appointment.Status = request.AppointmentStatus;
            appointment.ScheduleSlot.Status = request.ScheduleSlotStatus;

            // 4) Persiste alterações
            await _appointmentRepository.UpdateAsync(appointment);

            // 5) Monta response
            return new UpdateAppointmentStatusResponse(
                appointmentId: appointment.Id,
                scheduleSlotId: appointment.ScheduleSlot.Id,
                appointmentStatus: appointment.Status,
                scheduleSlotStatus: appointment.ScheduleSlot.Status
            );
        }
    }
}
