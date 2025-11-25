// Tests/TestData/Models/CompleteAppointmentGenerator.cs

using SGHSS.Application.UseCases.Appointments.Update;
using SGHSS.Application.UseCases.DigitalMedicalCertificates.Issue;
using SGHSS.Application.UseCases.EletronicPrescriptions.Issue;
using SGHSS.Application.UseCases.Patients.Update;
using SGHSS.Tests.TestData.Models;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.MedicalRecords;

namespace SGHSS.Tests.TestData.Appointments
{
    /// <summary>
    /// Classe utilitária responsável por gerar instâncias válidas e completas
    /// de <see cref="CompleteAppointmentRequest"/> para uso em cenários de teste.
    /// </summary>
    /// <remarks>
    /// Este generator reutiliza os generators já existentes para:
    /// <list type="bullet">
    /// <item><description>Atestados médicos digitais;</description></item>
    /// <item><description>Prescrições eletrônicas;</description></item>
    /// <item><description>Atualizações de prontuário.</description></item>
    /// </list>
    /// Todos os campos são opcionais, permitindo que cada cenário de teste
    /// controle exatamente quais documentos devem ser incluídos.
    /// </remarks>
    public static class CompleteAppointmentRequestGenerator
    {
        private static readonly Random Rng = new();

        /// <summary>
        /// Gera um request completo para conclusão de consulta,
        /// com ou sem requests filhos (atestados, prescrições e prontuário),
        /// de forma totalmente configurável.
        /// </summary>
        /// <param name="providedAppointmentId">
        /// ID da consulta alvo. Caso não informado, um Guid fictício será gerado.
        /// </param>
        /// <param name="includeCertificate">
        /// Indica se deve gerar automaticamente um request de atestado digital.
        /// </param>
        /// <param name="includePrescription">
        /// Indica se deve gerar automaticamente um request de prescrição eletrônica.
        /// </param>
        /// <param name="includeMedicalRecordUpdate">
        /// Indica se deve gerar automaticamente um request de atualização de prontuário.
        /// </param>
        /// <param name="certificateOverride">
        /// Caso informado, sobrescreve o request de atestado gerado automaticamente.
        /// </param>
        /// <param name="prescriptionOverride">
        /// Caso informado, sobrescreve o request de prescrição gerado automaticamente.
        /// </param>
        /// <param name="medicalRecordOverride">
        /// Caso informado, sobrescreve o request de prontuário gerado automaticamente.
        /// </param>
        /// <returns>
        /// Instância de <see cref="CompleteAppointmentRequest"/> pronta para uso no teste.
        /// </returns>
        public static CompleteAppointmentRequest Generate(
            Guid? providedAppointmentId = null,
            bool includeCertificate = false,
            bool includePrescription = false,
            bool includeMedicalRecordUpdate = false,
            IssueDigitalMedicalCertificateRequest? certificateOverride = null,
            IssueEletronicPrescriptionRequest? prescriptionOverride = null,
            UpdateMedicalRecordRequest? medicalRecordOverride = null
        )
        {
            Guid appointmentId = providedAppointmentId ?? Guid.NewGuid();

            IssueDigitalMedicalCertificateRequest? certificateRequest = null;
            IssueEletronicPrescriptionRequest? prescriptionRequest = null;
            UpdateMedicalRecordRequest? medicalRecordRequest = null;

            // 1) Atestado digital
            if (certificateOverride is not null)
            {
                certificateRequest = certificateOverride;
            }
            else if (includeCertificate)
            {
                certificateRequest = DigitalMedicalCertificateRequestGenerator.Generate(
                    providedAppointmentId: appointmentId
                );
            }

            // 2) Prescrição eletrônica
            if (prescriptionOverride is not null)
            {
                prescriptionRequest = prescriptionOverride;
            }
            else if (includePrescription)
            {
                prescriptionRequest = EletronicPrescriptionRequestGenerator.Generate(
                    providedAppointmentId: appointmentId
                );
            }

            // 3) Atualização de prontuário
            if (medicalRecordOverride is not null)
            {
                medicalRecordRequest = medicalRecordOverride;
            }
            else if (includeMedicalRecordUpdate)
            {
                medicalRecordRequest = UpdateMedicalRecordRequestGenerator.Generate();
            }

            // 4) Retorna o request final unificado
            return new CompleteAppointmentRequest
            {
                AppointmentId = appointmentId,
                DigitalMedicalCertificate = certificateRequest,
                EletronicPrescription = prescriptionRequest,
                MedicalRecordUpdate = medicalRecordRequest
            };
        }
    }
}
