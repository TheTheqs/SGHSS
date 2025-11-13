// Domain/Models/Patient.cs

using SGHSS.Domain.Enums;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Domain.Models
{
    /// <summary>
    /// Representa um paciente, incluindo informações pessoais de identificação,
    /// dados demográficos e contato para emergências.
    /// </summary>
    /// <remarks>
    /// Esta classe estende o tipo User para fornecer propriedades adicionais
    /// específicas de pacientes, como CPF, data de nascimento, sexo, endereço
    /// e informações de contato de emergência. Utilize este tipo para armazenar
    /// e gerenciar dados próprios de pacientes dentro do sistema.
    /// </remarks>
    public class Patient : User
    {
        public Cpf Cpf { get; set; }
        public DateTimeOffset BirthDate { get; set; }
        public Sex Sex { get; set; }
        public Address Address { get; set; }
        public string? EmergencyContactName { get; set; }

        public Patient() { }
    }
}
