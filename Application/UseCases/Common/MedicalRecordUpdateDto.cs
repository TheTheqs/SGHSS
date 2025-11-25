// Application/UseCases/Common/MedicalRecordUpdateDto.cs

namespace SGHSS.Application.UseCases.Common
{
    /// <summary>
    /// Representa uma atualização realizada em um prontuário médico.
    /// </summary>
    /// <remarks>
    /// Este DTO expõe apenas os dados necessários para exibição e auditoria
    /// em interface, mantendo a entidade de domínio protegida da camada externa.
    /// </remarks>
    public sealed class MedicalRecordUpdateDto
    {
        /// <summary>
        /// Identificador único da atualização de prontuário.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Data em que a atualização foi registrada.
        /// </summary>
        public DateTime UpdateDate { get; init; }

        /// <summary>
        /// Descrição textual da atualização realizada.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Identificador do profissional responsável pela atualização.
        /// </summary>
        public Guid ProfessionalId { get; init; }

        /// <summary>
        /// Identificador da unidade de saúde onde a atualização foi realizada.
        /// </summary>
        public Guid HealthUnitId { get; init; }

        /// <summary>
        /// Identificador da consulta associada à atualização, se houver.
        /// </summary>
        public Guid? AppointmentId { get; init; }
    }
}
