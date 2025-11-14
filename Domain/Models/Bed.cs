// Domain/Models/Bed.cs

using SGHSS.Domain.Enums;

namespace SGHSS.Domain.Models
{
    /// <summary>
    /// Representa um leito hospitalar, incluindo sua identificação, tipo, status e a internação atualmente vinculada.
    /// </summary>
    /// <remarks>Use esta classe para rastrear o estado e a alocação de leitos individuais dentro de uma unidade de
    /// saúde. O status do leito e a internação atual fornecem informações sobre sua disponibilidade e ocupação.</remarks>

    public class Bed
    {
        public Guid Id { get; set; }
        public string BedNumber { get; set; } = string.Empty;
        public BedType Type { get; set; }
        public BedStatus Status { get; set; }

        // Relacionamentos
        public Hospitalization? CurrentHospitalization { get; set; }

        // Construtor padrão
        public Bed() { }
    }
}
