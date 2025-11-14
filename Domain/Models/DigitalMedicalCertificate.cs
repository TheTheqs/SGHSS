// Domain/Models/DigitalMedicalCertificate.cs

using SGHSS.Domain.ValueObjects;

namespace SGHSS.Domain.Models
{
    /// <summary>
    /// Representa um atestado médico digital emitido para um paciente, incluindo detalhes de emissão,
    /// período de validade, recomendações e entidades associadas.
    /// </summary>
    /// <remarks>Um atestado médico digital normalmente serve como registro oficial de uma avaliação médica,
    /// frequentemente utilizado para afastamento do trabalho, seguros ou fins legais. Esta classe encapsula
    /// as informações essenciais e os relacionamentos necessários para representar tal atestado em um sistema de saúde.</remarks>

    public class DigitalMedicalCertificate
    {
        public Guid Id { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime ValidUntil { get; set; }
        public string Recommendations { get; set; } = string.Empty;
        public IcpSignature IcpSignature { get; set; }

        // Relacionamentos
        public Appointment Appointment { get; set; } = null!;
        public Patient Patient { get; set; } = null!;
        public Professional Professional { get; set; } = null!;

        // Construtor padrão
        public DigitalMedicalCertificate() { }
    }
}
