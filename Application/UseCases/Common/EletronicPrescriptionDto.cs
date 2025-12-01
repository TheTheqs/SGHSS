// Application/UseCases/Common/EletronicPrescriptionDto.cs


namespace SGHSS.Application.UseCases.Common
{
    /// <summary>
    /// Representa uma visão resumida de uma prescrição eletrônica emitida
    /// para um paciente, contendo apenas dados necessários para exibição
    /// e consulta em camadas superiores.
    /// </summary>
    public sealed class EletronicPrescriptionDto
    {
        /// <summary>
        /// Identificador único da prescrição eletrônica.
        /// </summary>
        public Guid PrescriptionId { get; init; }

        /// <summary>
        /// Identificador do paciente associado à prescrição.
        /// </summary>
        public Guid PatientId { get; init; }

        /// <summary>
        /// Identificador do profissional responsável pela prescrição.
        /// </summary>
        public Guid ProfessionalId { get; init; }

        /// <summary>
        /// Identificador da unidade de saúde em que a prescrição foi emitida.
        /// </summary>
        public Guid HealthUnitId { get; init; }

        /// <summary>
        /// Identificador da consulta associada à prescrição, quando aplicável.
        /// </summary>
        public Guid AppointmentId { get; init; }

        /// <summary>
        /// Data e hora em que a prescrição foi criada (normalmente em UTC).
        /// </summary>
        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>
        /// Data e hora até a qual a prescrição é considerada válida.
        /// </summary>
        public DateTimeOffset ValidUntil { get; init; }

        /// <summary>
        /// Instruções clínicas, posologia ou orientações registradas
        /// na prescrição eletrônica.
        /// </summary>
        public string Instructions { get; init; } = string.Empty;
    }
}
