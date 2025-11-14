// Domain/Models/HealthUnit.cs

using SGHSS.Domain.Enums;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Domain.Models
{
    /// <summary>
    /// Representa uma unidade de saúde, como uma clínica ou hospital, incluindo sua identificação,
    /// informações de contato e os profissionais e pacientes associados.
    /// </summary>
    /// <remarks>
    /// Uma unidade de saúde normalmente atua como uma entidade central em sistemas de atendimento,
    /// oferecendo serviços a pacientes e empregando profissionais da área. Esta classe encapsula
    /// os principais detalhes sobre a unidade, incluindo seu identificador único, dados de registro,
    /// endereço e relacionamentos com profissionais e pacientes.
    /// </remarks>
    public class HealthUnit
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Cnpj Cnpj { get; set; }
        public Address Address { get; set; }
        public Phone Phone { get; set; }
        public HealthUnitType Type { get; set; }

        // Relacionamentos
        public ICollection<Professional> Professionals { get; set; } = new List<Professional>();
        public ICollection<Patient> Patients { get; set; } = new List<Patient>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<MedicalRecordUpdate> MedicalRecordUpdates { get; set; } = new List<MedicalRecordUpdate>();
        public ICollection<EletronicPrescription> EletronicPrescriptions { get; set; } = new List<EletronicPrescription>();
        public ICollection<DigitalMedicalCertificate> DigitalMedicalCertificates { get; set; } = new List<DigitalMedicalCertificate>();
        public ICollection<HomeCare> HomeCares { get; set; } = new List<HomeCare>();
        public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
        public ICollection<Bed> Beds { get; set; } = new List<Bed>();
        public ICollection<LogActivity> LogActivities { get; set; } = new List<LogActivity>();


        // Construtor padrão
        public HealthUnit() { }
    }
}
