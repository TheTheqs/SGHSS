// Domain/Models/HealthUnit.cs

using SGHSS.Domain.Enums;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Domain.Models
{
    /// <summary>
    /// Represents a health unit, such as a clinic or hospital, including its identification, contact information, and
    /// associated professionals and patients.
    /// </summary>
    /// <remarks>A health unit typically serves as a central entity in healthcare systems, providing services
    /// to patients and employing healthcare professionals. This class encapsulates key details about the unit,
    /// including its unique identifier, registration information, address, and relationships to professionals and
    /// patients.</remarks>
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


        // Construtor padrão
        public HealthUnit() { }
    }
}
