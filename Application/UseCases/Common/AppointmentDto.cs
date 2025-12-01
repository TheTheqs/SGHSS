// Application/UseCases/Common/AppointmentDto.cs

using SGHSS.Domain.Enums;

namespace SGHSS.Application.UseCases.Common
{
    /// <summary>
    /// Representa os dados resumidos de uma consulta (appointment)
    /// para exibição em listagens ou relatórios.
    /// </summary>
    public sealed class AppointmentDto
    {
        /// <summary>
        /// Identificador único da consulta.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Data e hora de início da consulta.
        /// </summary>
        public DateTimeOffset StartTime { get; init; }

        /// <summary>
        /// Data e hora de término da consulta.
        /// </summary>
        public DateTimeOffset EndTime { get; init; }

        /// <summary>
        /// Status atual da consulta (agendada, concluída, cancelada, etc.).
        /// </summary>
        public AppointmentStatus Status { get; init; }

        /// <summary>
        /// Tipo da consulta (presencial, teleconsulta, retorno, etc.).
        /// </summary>
        public AppointmentType Type { get; init; }

        /// <summary>
        /// Link associado à consulta, quando se tratar de atendimento remoto
        /// (por exemplo, uma URL de videoconferência).
        /// </summary>
        public string? Link { get; init; }

        /// <summary>
        /// Descrição ou observações associadas à consulta.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Indica se esta consulta já possui uma atualização de prontuário
        /// diretamente vinculada a ela.
        /// </summary>
        public bool HasMedicalRecordUpdate { get; init; }

        /// <summary>
        /// Indica se esta consulta já possui uma prescrição eletrônica
        /// associada.
        /// </summary>
        public bool HasEletronicPrescription { get; init; }

        /// <summary>
        /// Indica se esta consulta já possui um atestado médico digital
        /// associado.
        /// </summary>
        public bool HasDigitalMedicalCertificate { get; init; }
    }
}
