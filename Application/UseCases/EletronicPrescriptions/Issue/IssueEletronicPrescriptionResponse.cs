// Application/UseCases/EletronicPrescriptions/Issue/IssueEletronicPrescriptionResponse.cs

namespace SGHSS.Application.UseCases.EletronicPrescriptions.Issue
{
    /// <summary>
    /// Representa o resultado da emissão de uma prescrição eletrônica.
    /// </summary>
    /// <remarks>
    /// Este response expõe os identificadores principais da prescrição emitida,
    /// bem como metadados relevantes para exibição ou auditoria em camadas superiores.
    /// </remarks>
    public sealed class IssueEletronicPrescriptionResponse
    {
        /// <summary>
        /// Identificador único da prescrição eletrônica emitida.
        /// </summary>
        public Guid PrescriptionId { get; }

        /// <summary>
        /// Identificador do paciente associado à prescrição.
        /// </summary>
        public Guid PatientId { get; }

        /// <summary>
        /// Identificador do profissional que emitiu a prescrição.
        /// </summary>
        public Guid ProfessionalId { get; }

        /// <summary>
        /// Identificador da unidade de saúde em que a prescrição foi emitida.
        /// </summary>
        public Guid HealthUnitId { get; }

        /// <summary>
        /// Identificador da consulta associada à prescrição.
        /// </summary>
        public Guid AppointmentId { get; }

        /// <summary>
        /// Data e hora em que a prescrição foi criada.
        /// </summary>
        public DateTimeOffset CreatedAt { get; }

        /// <summary>
        /// Data e hora até a qual a prescrição é considerada válida.
        /// </summary>
        public DateTimeOffset ValidUntil { get; }

        /// <summary>
        /// Instruções clínicas registradas na prescrição.
        /// </summary>
        public string Instructions { get; }

        /// <summary>
        /// Cria uma nova instância de response para emissão de prescrição eletrônica.
        /// </summary>
        public IssueEletronicPrescriptionResponse(
            Guid prescriptionId,
            Guid patientId,
            Guid professionalId,
            Guid healthUnitId,
            Guid appointmentId,
            DateTimeOffset createdAt,
            DateTimeOffset validUntil,
            string instructions)
        {
            PrescriptionId = prescriptionId;
            PatientId = patientId;
            ProfessionalId = professionalId;
            HealthUnitId = healthUnitId;
            AppointmentId = appointmentId;
            CreatedAt = createdAt;
            ValidUntil = validUntil;
            Instructions = instructions;
        }
    }
}
