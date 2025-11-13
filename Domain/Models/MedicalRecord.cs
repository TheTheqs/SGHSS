// Domain/Models/MedicalRecord.cs

using SGHSS.Domain.ValueObjects;

namespace SGHSS.Domain.Models
{
    /// <summary>
    /// Representa o prontuário de um paciente, incluindo informações de identificação,
    /// dados associados ao paciente e um histórico de atualizações.
    /// </summary>
    /// <remarks>
    /// Um prontuário geralmente contém informações sobre o histórico médico do paciente,
    /// tratamentos e atualizações ao longo do tempo. Esta classe atua como um contêiner
    /// para os dados centrais e relacionamentos vinculados a um único prontuário dentro do sistema.
    /// </remarks>
    public class MedicalRecord
    {
        public Guid Id { get; set; }
        public MedicalRecordNumber Number { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        // Relacionamentos
        public Patient Patient { get; set; } = null!;
        public ICollection<MedicalRecordUpdate> Updates { get; set; } = new List<MedicalRecordUpdate>();

        // Construtor padrão
        public MedicalRecord() { }
    }
}
