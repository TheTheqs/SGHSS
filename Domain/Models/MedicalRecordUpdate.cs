// Domain/Models/MedicalRecordUpdate.cs

namespace SGHSS.Domain.Models
{
    /// <summary>
    /// Representa um registro de atualização associado a um prontuário, incluindo detalhes da alteração,
    /// o profissional responsável e as entidades relacionadas.
    /// </summary>
    /// <remarks>
    /// Um MedicalRecordUpdate normalmente registra mudanças, anotações ou ações realizadas no prontuário
    /// de um paciente. Ele se vincula ao prontuário original, ao profissional que efetuou a atualização,
    /// à unidade de saúde onde ocorreu a alteração e, opcionalmente, a uma consulta relacionada.
    /// Essa classe é utilizada para acompanhar o histórico de modificações ou adições às informações
    /// médicas de um paciente.
    /// </remarks>
    public class MedicalRecordUpdate
    {
        public Guid Id { get; set; }
        public DateTime UpdateDate { get; set; }
        public string Description { get; set; } = null!;

        // Relacionamentos
        public MedicalRecord MedicalRecord { get; set; } = null!;
        public Professional Professional { get; set; } = null!;
        public HealthUnit HealthUnit { get; set; } = null!;
        public Appointment? Appointment { get; set; }

        // Construtor padrão
        public MedicalRecordUpdate() { }
    }
}
