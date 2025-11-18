// Application/UseCases/Patients/Register/RegisterPatientRequest.cs

using SGHSS.Application.UseCases.Common;
using SGHSS.Domain.Enums;

namespace SGHSS.Application.UseCases.Patients.Register
{
    /// <summary>
    /// Representa os dados de entrada necessários para registrar um novo paciente no sistema.
    /// </summary>
    /// <remarks>
    /// Este DTO é utilizado pela camada de Application para receber as informações enviadas
    /// pela camada de interface (por exemplo, uma API) e orquestrar o caso de uso de registro
    /// de pacientes, sem expor diretamente as entidades de domínio.
    /// </remarks>
    public sealed class RegisterPatientRequest
    {
        /// <summary>
        /// Nome completo do paciente.
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Endereço de e-mail do paciente.
        /// </summary>
        public string Email { get; init; } = string.Empty;

        /// <summary>
        /// Número de telefone do paciente (em formato normalizado pela camada de interface).
        /// </summary>
        public string Phone { get; init; } = string.Empty;

        /// <summary>
        /// CPF do paciente em formato textual (apenas dígitos ou com máscara, conforme convenção da aplicação).
        /// </summary>
        public string Cpf { get; init; } = string.Empty;

        /// <summary>
        /// Data de nascimento do paciente.
        /// </summary>
        public DateTimeOffset BirthDate { get; init; }

        /// <summary>
        /// Sexo do paciente.
        /// </summary>
        public Sex Sex { get; init; }

        /// <summary>
        /// Endereço residencial do paciente.
        /// </summary>
        public AddressDto Address { get; init; } = null!;

        /// <summary>
        /// Nome do contato de emergência, caso informado.
        /// </summary>
        public string? EmergencyContactName { get; init; }

        /// <summary>
        /// Identificador opcional da unidade de saúde principal associada ao paciente.
        /// </summary>
        public Guid? PrimaryHealthUnitId { get; init; }
    }
}
