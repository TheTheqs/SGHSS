// Application/UseCases/DigitalMedicalCertificates/Issue/IssueDigitalMedicalCertificateResponse.cs

namespace SGHSS.Application.UseCases.DigitalMedicalCertificates.Issue
{
    /// <summary>
    /// Representa o resultado da emissão de um atestado médico digital.
    /// </summary>
    /// <remarks>
    /// Este response expõe os identificadores principais do atestado emitido,
    /// bem como metadados relevantes para exibição ou auditoria em camadas superiores.
    /// </remarks>
    public sealed class IssueDigitalMedicalCertificateResponse
    {
        /// <summary>
        /// Identificador único do atestado médico digital emitido.
        /// </summary>
        public Guid CertificateId { get; }

        /// <summary>
        /// Identificador do paciente associado ao atestado.
        /// </summary>
        public Guid PatientId { get; }

        /// <summary>
        /// Identificador do profissional que emitiu o atestado.
        /// </summary>
        public Guid ProfessionalId { get; }

        /// <summary>
        /// Identificador da unidade de saúde em que o atestado foi emitido.
        /// </summary>
        public Guid HealthUnitId { get; }

        /// <summary>
        /// Identificador da consulta associada ao atestado.
        /// </summary>
        public Guid AppointmentId { get; }

        /// <summary>
        /// Data e hora em que o atestado foi emitido.
        /// </summary>
        public DateTimeOffset IssuedAt { get; }

        /// <summary>
        /// Data e hora até a qual o atestado é considerado válido.
        /// </summary>
        public DateTimeOffset ValidUntil { get; }

        /// <summary>
        /// Recomendações clínicas registradas no atestado.
        /// </summary>
        public string Recommendations { get; }

        /// <summary>
        /// Cria uma nova instância de response para emissão de atestado médico digital.
        /// </summary>
        public IssueDigitalMedicalCertificateResponse(
            Guid certificateId,
            Guid patientId,
            Guid professionalId,
            Guid healthUnitId,
            Guid appointmentId,
            DateTimeOffset issuedAt,
            DateTimeOffset validUntil,
            string recommendations)
        {
            CertificateId = certificateId;
            PatientId = patientId;
            ProfessionalId = professionalId;
            HealthUnitId = healthUnitId;
            AppointmentId = appointmentId;
            IssuedAt = issuedAt;
            ValidUntil = validUntil;
            Recommendations = recommendations;
        }
    }
}
