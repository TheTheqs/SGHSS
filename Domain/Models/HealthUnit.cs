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


        // Construtor padrão
        public HealthUnit() { }
    }
}
