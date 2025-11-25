// Application/UseCases/Appointments/Update/CompleteAppointmentUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.DigitalMedicalCertificates.Issue;
using SGHSS.Application.UseCases.EletronicPrescriptions.Issue;
using SGHSS.Application.UseCases.Patients.Update;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;

namespace SGHSS.Application.UseCases.Appointments.Update
{
    /// <summary>
    /// Caso de uso responsável por concluir uma consulta,
    /// permitindo a emissão opcional de documentos clínicos associados,
    /// como atestado digital, prescrição eletrônica e atualização de prontuário.
    /// </summary>
    /// <remarks>
    /// Este caso de uso parte de uma consulta previamente agendada e confirmada.
    /// Ao final da operação, a consulta tem o seu status atualizado para
    /// <see cref="AppointmentStatus.Completed"/> e o slot de agenda associado
    /// também é marcado como concluído.
    /// </remarks>
    public sealed class CompleteAppointmentUseCase
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IssueDigitalMedicalCertificateUseCase _issueDigitalMedicalCertificateUseCase;
        private readonly IssueEletronicPrescriptionUseCase _issueEletronicPrescriptionUseCase;
        private readonly UpdateMedicalRecordUseCase _updateMedicalRecordUseCase;

        /// <summary>
        /// Cria uma nova instância do caso de uso de conclusão de consulta.
        /// </summary>
        /// <param name="appointmentRepository">
        /// Repositório responsável pelo acesso e persistência de consultas.
        /// </param>
        /// <param name="issueDigitalMedicalCertificateUseCase">
        /// Caso de uso especializado na emissão de atestados médicos digitais.
        /// </param>
        /// <param name="issueEletronicPrescriptionUseCase">
        /// Caso de uso especializado na emissão de prescrições eletrônicas.
        /// </param>
        /// <param name="updateMedicalRecordUseCase">
        /// Caso de uso especializado na atualização do prontuário médico de pacientes.
        /// </param>
        public CompleteAppointmentUseCase(
            IAppointmentRepository appointmentRepository,
            IssueDigitalMedicalCertificateUseCase issueDigitalMedicalCertificateUseCase,
            IssueEletronicPrescriptionUseCase issueEletronicPrescriptionUseCase,
            UpdateMedicalRecordUseCase updateMedicalRecordUseCase)
        {
            _appointmentRepository = appointmentRepository;
            _issueDigitalMedicalCertificateUseCase = issueDigitalMedicalCertificateUseCase;
            _issueEletronicPrescriptionUseCase = issueEletronicPrescriptionUseCase;
            _updateMedicalRecordUseCase = updateMedicalRecordUseCase;
        }

        /// <summary>
        /// Conclui uma consulta previamente agendada, permitindo a emissão opcional
        /// de documentos clínicos associados.
        /// </summary>
        /// <param name="request">
        /// Dados necessários para identificar a consulta e, opcionalmente,
        /// emitir documentos clínicos vinculados a ela.
        /// </param>
        /// <returns>
        /// Um <see cref="CompleteAppointmentResponse"/> contendo o status final da consulta
        /// e os identificadores dos registros clínicos gerados (quando houver).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Lançada quando o <paramref name="request"/> é nulo.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando a consulta não é encontrada ou está em um estado
        /// que não permite conclusão.
        /// </exception>
        public async Task<CompleteAppointmentResponse> Handle(CompleteAppointmentRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            // 1) Carrega a consulta
            Appointment? appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId);

            if (appointment is null)
                throw new InvalidOperationException("Consulta informada não foi encontrada.");

            if (appointment.ScheduleSlot is null)
                throw new InvalidOperationException("O slot de agenda associado à consulta não foi carregado.");

            // Apenas consultas confirmadas podem ser concluídas
            if (appointment.Status != AppointmentStatus.Confirmed)
                throw new InvalidOperationException("Apenas consultas confirmadas podem ser concluídas.");

            // Variáveis para armazenar os IDs dos registros clínicos gerados
            Guid? digitalMedicalCertificateId = null;
            Guid? eletronicPrescriptionId = null;
            Guid? medicalRecordUpdateId = null;

            // 2) Emissão opcional de atestado médico digital
            if (request.DigitalMedicalCertificate is not null)
            {
                var certificateRequest = new IssueDigitalMedicalCertificateRequest
                {
                    PatientId = request.DigitalMedicalCertificate.PatientId,
                    ProfessionalId = request.DigitalMedicalCertificate.ProfessionalId,
                    HealthUnitId = request.DigitalMedicalCertificate.HealthUnitId,
                    AppointmentId = appointment.Id, // força consistência
                    ValidUntil = request.DigitalMedicalCertificate.ValidUntil,
                    Recommendations = request.DigitalMedicalCertificate.Recommendations,
                    IcpSignatureRaw = request.DigitalMedicalCertificate.IcpSignatureRaw
                };

                var certificateResponse =
                    await _issueDigitalMedicalCertificateUseCase.Handle(certificateRequest);

                // Pressupõe que o response expose o ID do atestado criado
                digitalMedicalCertificateId = certificateResponse.CertificateId;
            }

            // 3) Emissão opcional de prescrição eletrônica
            if (request.EletronicPrescription is not null)
            {
                var prescriptionRequest = new IssueEletronicPrescriptionRequest
                {
                    PatientId = request.EletronicPrescription.PatientId,
                    ProfessionalId = request.EletronicPrescription.ProfessionalId,
                    HealthUnitId = request.EletronicPrescription.HealthUnitId,
                    AppointmentId = appointment.Id, // força consistência
                    ValidUntil = request.EletronicPrescription.ValidUntil,
                    Instructions = request.EletronicPrescription.Instructions,
                    IcpSignatureRaw = request.EletronicPrescription.IcpSignatureRaw
                };

                var prescriptionResponse =
                    await _issueEletronicPrescriptionUseCase.Handle(prescriptionRequest);

                // Pressupõe que o response expose o ID da prescrição criada
                eletronicPrescriptionId = prescriptionResponse.PrescriptionId;
            }

            // 4) Atualização opcional de prontuário médico
            if (request.MedicalRecordUpdate is not null)
            {
                var medicalRecordRequest = new UpdateMedicalRecordRequest
                {
                    PatientId = request.MedicalRecordUpdate.PatientId,
                    ProfessionalId = request.MedicalRecordUpdate.ProfessionalId,
                    HealthUnitId = request.MedicalRecordUpdate.HealthUnitId,
                    AppointmentId = appointment.Id, // força consistência
                    Description = request.MedicalRecordUpdate.Description
                };

                var medicalRecordResponse =
                    await _updateMedicalRecordUseCase.Handle(medicalRecordRequest);

                // Pressupõe que o response expose o ID da atualização criada
                medicalRecordUpdateId = medicalRecordResponse.MedicalRecordUpdateId;
            }

            // 5) Atualiza status da consulta e do slot
            appointment.Status = AppointmentStatus.Completed;
            appointment.ScheduleSlot.Status = ScheduleSlotStatus.Completed;

            await _appointmentRepository.UpdateAsync(appointment);

            // 6) Monta o response
            return new CompleteAppointmentResponse
            {
                AppointmentId = appointment.Id,
                Status = appointment.Status,
                ScheduleSlotId = appointment.ScheduleSlot.Id,
                MedicalRecordUpdateId = medicalRecordUpdateId,
                EletronicPrescriptionId = eletronicPrescriptionId,
                DigitalMedicalCertificateId = digitalMedicalCertificateId
            };
        }
    }
}
