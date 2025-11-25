// Application/UseCases/Patients/ConsultMedicalRecord/MedicalRecordDto.cs

namespace SGHSS.Application.UseCases.Common
{
    /// <summary>
    /// Representa os dados de um prontuário médico retornados para a camada de interface.
    /// </summary>
    /// <remarks>
    /// Este DTO contém informações de identificação do prontuário, metadados
    /// básicos (como data de criação) e a lista de atualizações realizadas ao longo do tempo.
    /// </remarks>
    public sealed class MedicalRecordDto
    {
        /// <summary>
        /// Identificador único do prontuário.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Número do prontuário em formato textual.
        /// </summary>
        /// <remarks>
        /// O valor é derivado do Value Object de número de prontuário definido no domínio
        /// e serializado para texto para consumo pela interface.
        /// </remarks>
        public string Number { get; init; } = string.Empty;

        /// <summary>
        /// Data e hora em que o prontuário foi criado no sistema.
        /// </summary>
        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>
        /// Coleção de atualizações realizadas neste prontuário ao longo do tempo.
        /// </summary>
        public ICollection<MedicalRecordUpdateDto> Updates { get; init; } = new List<MedicalRecordUpdateDto>();
    }
}
