// Application/UseCases/Administrators/Register/RegisterPatientRequest.cs


using SGHSS.Application.UseCases.Common;
using SGHSS.Domain.Enums;

namespace SGHSS.Application.UseCases.Administrators.Register
{
    /// <summary>
    /// Representa os dados necessários para registrar um novo Administrador no sistema,
    /// fornecendo informações básicas de identificação e o nível de acesso associado.
    /// </summary>

    public sealed class RegisterAdministratorRequest
    {
        /// <summary>
        /// Nome completo do Administrador.
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Endereço de e-mail do Administrador.
        /// </summary>
        public string Email { get; init; } = string.Empty;

        /// <summary>
        /// Senha pura fornecida no cadastro, que será transformada em um Value Object Password no UseCase.
        /// </summary>
        public string Password { get; init; } = string.Empty;

        /// <summary>
        /// Número de telefone do Administrador (em formato normalizado pela camada de interface).
        /// </summary>
        public string Phone { get; init; } = string.Empty;

        /// <summary>
        /// O nível de acesso do Administrador.
        /// </summary>
        public AccessLevel AccessLevel { get; init; }

        /// <summary>
        /// Consentimentos associados ao contexto atual.
        /// </summary>
        public ICollection<ConsentDto> Consents { get; init; } = new List<ConsentDto>();
    }
}
