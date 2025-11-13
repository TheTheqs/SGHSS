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
        public ProfessionalLicense License { get; set; }
        public string Specialty { get; set; } = string.Empty;
        public Availability Availability { get; set; }

        // Relacionamentos
        public ICollection<HealthUnit> HealthUnits { get; set; } = new List<HealthUnit>();
        public ProfessionalSchedule ProfessionalSchedule { get; set; } = null!;
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        //Construtor padrão
        public Professional() {}
    }
}
