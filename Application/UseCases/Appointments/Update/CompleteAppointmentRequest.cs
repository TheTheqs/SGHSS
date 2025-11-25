// Application/UseCases/Appointments/Update/CompleteAppointmentRequest.cs

using SGHSS.Application.UseCases.DigitalMedicalCertificates.Issue;
using SGHSS.Application.UseCases.EletronicPrescriptions.Issue;
using SGHSS.Application.UseCases.Patients.Update;

namespace SGHSS.Application.UseCases.Appointments.Update
{
    /// <summary>
    /// Representa os dados necessários para concluir uma consulta,
    /// incluindo a possibilidade de emitir documentos clínicos associados.
    /// </summary>
    /// <remarks>
    /// Este request parte do pressuposto de que a consulta já foi previamente agendada
    /// e que o agregado <see cref="Domain.Models.Appointment"/> pode ser carregado
    /// a partir do identificador informado.
    /// <para>
    /// Opcionalmente, podem ser fornecidos requests para emissão de:
    /// <list type="bullet">
    /// <item><description>Atestado médico digital;</description></item>
    /// <item><description>Prescrição eletrônica;</description></item>
    /// <item><description>Atualização de prontuário médico.</description></item>
    /// </list>
    /// Quando presentes, esses requests serão utilizados para invocar os respectivos
    /// casos de uso especializados durante a conclusão da consulta.
    /// </para>
    /// </remarks>
    public sealed class CompleteAppointmentRequest
    {
        /// <summary>
        /// Identificador da consulta a ser concluída.
        /// </summary>
        /// <remarks>
        /// A partir deste identificador, o caso de uso deve carregar o agregado
        /// <see cref="Domain.Models.Appointment"/> e seu <see cref="Domain.Models.ScheduleSlot"/>
        /// associado.
        /// </remarks>
        public Guid AppointmentId { get; init; }

        /// <summary>
        /// Dados para emissão de um atestado médico digital vinculado à consulta,
        /// quando aplicável.
        /// </summary>
        /// <remarks>
        /// Quando informado, o caso de uso poderá delegar ao
        /// <c>IssueDigitalMedicalCertificateUseCase</c> a criação do atestado.
        /// </remarks>
        public IssueDigitalMedicalCertificateRequest? DigitalMedicalCertificate { get; init; }

        /// <summary>
        /// Dados para emissão de uma prescrição eletrônica vinculada à consulta,
        /// quando aplicável.
        /// </summary>
        /// <remarks>
        /// Quando informado, o caso de uso poderá delegar ao
        /// <c>IssueEletronicPrescriptionUseCase</c> a criação da prescrição.
        /// </remarks>
        public IssueEletronicPrescriptionRequest? EletronicPrescription { get; init; }

        /// <summary>
        /// Dados para registrar uma atualização de prontuário médico vinculada
        /// à consulta, quando aplicável.
        /// </summary>
        /// <remarks>
        /// Quando informado, o caso de uso poderá delegar ao
        /// <c>UpdateMedicalRecordUseCase</c> o registro da atualização.
        /// </remarks>
        public UpdateMedicalRecordRequest? MedicalRecordUpdate { get; init; }
    }
}
