// Application/UseCases/Common/DigitalMedicalCertificateDto.cs

namespace SGHSS.Application.UseCases.Common
{
    /// <summary>
    /// Representa uma visão resumida de um atestado médico digital emitido
    /// para um paciente, contendo apenas dados necessários para exibição
    /// e consulta em camadas superiores.
    /// </summary>
    public sealed class DigitalMedicalCertificateDto
    {
        /// <summary>
        /// Identificador único do atestado médico digital.
        /// </summary>
        public Guid CertificateId { get; init; }

        /// <summary>
        /// Identificador do paciente para o qual o atestado foi emitido.
        /// </summary>
        public Guid PatientId { get; init; }

        /// <summary>
        /// Identificador do profissional responsável pela emissão do atestado.
        /// </summary>
        public Guid ProfessionalId { get; init; }

        /// <summary>
        /// Identificador da unidade de saúde em que o atestado foi emitido.
        /// </summary>
        public Guid HealthUnitId { get; init; }

        /// <summary>
        /// Identificador da consulta associada a este atestado, quando aplicável.
        /// </summary>
        public Guid AppointmentId { get; init; }

        /// <summary>
        /// Data e hora em que o atestado foi criado (normalmente em UTC).
        /// </summary>
        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>
        /// Data e hora até a qual o atestado é considerado válido.
        /// </summary>
        public DateTimeOffset ValidUntil { get; init; }

        /// <summary>
        /// Recomendações clínicas, observações ou orientações registradas
        /// no atestado médico digital.
        /// </summary>
        public string Recommendations { get; init; } = string.Empty;
    }
}
