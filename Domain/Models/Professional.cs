// Domain/Models/Professional.cs

using SGHSS.Domain.Enums;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Domain.Models
{
    /// <summary>
    /// Representa um profissional da área da saúde, incluindo suas credenciais
    /// profissionais, especialidade médica e informações de disponibilidade.
    /// </summary>
    /// <remarks>
    /// Esta classe estende o tipo User para incluir propriedades adicionais
    /// relacionadas à atuação clínica, como o registro profissional, especialidade
    /// e status de disponibilidade para atendimentos.
    /// </remarks>
    public class Professional : User
    {
        public ProfessionalLicense License { get; set; } // Value Object
        public string Specialty { get; set; } = string.Empty;
        public Availability Availability { get; set; } // Enum

        // Relacionamentos
        public ICollection<HealthUnit> HealthUnits { get; set; } = new List<HealthUnit>();
        public ProfessionalSchedule ProfessionalSchedule { get; set; } = null!;
        public ICollection<MedicalRecordUpdate> MedicalRecordUpdates { get; set; } = new List<MedicalRecordUpdate>();
        public ICollection<EletronicPrescription> EletronicPrescriptions { get; set; } = new List<EletronicPrescription>();
        public ICollection<DigitalMedicalCertificate> DigitalMedicalCertificates { get; set; } = new List<DigitalMedicalCertificate>();
        public ICollection<HomeCare> HomeCares { get; set; } = new List<HomeCare>();


        //Construtor padrão
        public Professional() {}
    }
}
