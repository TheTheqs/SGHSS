// Application/UseCases/Appointments/Update/CompleteAppointmentResponse.cs

using SGHSS.Domain.Enums;

namespace SGHSS.Application.UseCases.Appointments.Update
{
    /// <summary>
    /// Representa o resultado da conclusão de uma consulta,
    /// incluindo o status final e os identificadores dos registros clínicos
    /// eventualmente gerados no processo.
    /// </summary>
    public sealed class CompleteAppointmentResponse
    {
        /// <summary>
        /// Identificador da consulta concluída.
        /// </summary>
        public Guid AppointmentId { get; init; }

        /// <summary>
        /// Status final da consulta após a conclusão.
        /// </summary>
        /// <remarks>
        /// Em um fluxo bem-sucedido, o valor esperado é <see cref="AppointmentStatus.Completed"/>.
        /// </remarks>
        public AppointmentStatus Status { get; init; }

        /// <summary>
        /// Identificador do slot de agenda associado à consulta.
        /// </summary>
        /// <remarks>
        /// O slot deverá ter seu status atualizado para refletir a conclusão do atendimento.
        /// </remarks>
        public Guid ScheduleSlotId { get; init; }

        /// <summary>
        /// Identificador da atualização de prontuário criada, quando aplicável.
        /// </summary>
        public Guid? MedicalRecordUpdateId { get; init; }

        /// <summary>
        /// Identificador da prescrição eletrônica emitida, quando aplicável.
        /// </summary>
        public Guid? EletronicPrescriptionId { get; init; }

        /// <summary>
        /// Identificador do atestado médico digital emitido, quando aplicável.
        /// </summary>
        public Guid? DigitalMedicalCertificateId { get; init; }
    }
}
