// Domain/Models/Hospitalization.cs

using SGHSS.Domain.Enums;

namespace SGHSS.Domain.Models
{
    /// <summary>
    /// Representa o registro da permanência de um paciente em um hospital, incluindo detalhes de admissão e alta,
    /// motivo da internação e informações associadas ao paciente e ao leito.
    /// </summary>
    /// <remarks>Uma instância de internação acompanha todo o ciclo da admissão de um paciente, desde a data inicial
    /// de entrada até a alta, mantendo referências ao paciente e ao leito designado. A propriedade de status indica
    /// o estado atual da internação (como ativa ou concluída).</remarks>

    public class Hospitalization
    {
        public Guid Id { get; set; }
        public DateTimeOffset AdmissionDate { get; set; }
        public DateTimeOffset? DischargeDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public HospitalizationStatus Status { get; set; } // Enum

        // Relacionamentos
        public Patient Patient { get; set; } = null!;
        public Bed Bed { get; set; } = null!;

        // Construtor padrão
        public Hospitalization() { }
    }
}
