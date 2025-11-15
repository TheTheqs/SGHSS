// Domain/Models/EletronicPrescription.cs

using SGHSS.Domain.ValueObjects;

namespace SGHSS.Domain.Models
{
    /// <summary>
    /// Representa uma prescrição eletrônica, incluindo detalhes da prescrição, o paciente associado,
    /// o profissional de saúde e as informações da consulta relacionada.
    /// </summary>
    /// <remarks>Uma prescrição eletrônica contém todos os dados necessários para identificar a receita,
    /// seu período de validade e a assinatura digital exigida para conformidade legal. Ela também mantém
    /// referências ao paciente, ao profissional prescritor e à consulta durante a qual a prescrição foi emitida.</remarks>

    public class EletronicPrescription
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ValidUntil { get; set; }
        public string Instructions { get; set; } = string.Empty;
        public IcpSignature IcpSignature { get; set; } // Value Object

        // Relacionamentos
        public Appointment Appointment { get; set; } = null!;
        public Patient Patient { get; set; } = null!;
        public Professional Professional { get; set; } = null!;
        public HealthUnit HealthUnit { get; set; } = null!;

        // Construtor padrão
        public EletronicPrescription() { }
    }
}
