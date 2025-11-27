// Application/UseCases/Authentication/AuthenticateUserResponse.cs

using SGHSS.Domain.Enums;

namespace SGHSS.Application.UseCases.Authentication
{
    /// <summary>
    /// Representa o resultado da autenticação de um usuário, incluindo
    /// o token de acesso e informações de identificação e permissão.
    /// </summary>
    /// <remarks>
    /// Este response é retornado pelo caso de uso de autenticação quando
    /// as credenciais são válidas. Ele contém o token JWT gerado, bem como
    /// dados básicos do usuário e o seu nível de acesso efetivo no sistema.
    /// </remarks>
    public class AuthenticateUserResponse
    {
        /// <summary>
        /// Identificador único do usuário autenticado.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Nome do usuário autenticado.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Endereço de e-mail do usuário autenticado.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Tipo concreto do usuário no domínio, por exemplo:
        /// <c>Administrator</c>, <c>Patient</c> ou <c>Professional</c>.
        /// </summary>
        public string UserType { get; set; } = string.Empty;

        /// <summary>
        /// Nível de acesso efetivo do usuário no sistema.
        /// </summary>
        /// <remarks>
        /// Para pacientes, este valor será <see cref="AccessLevel.Patient"/>.
        /// Para profissionais, <see cref="AccessLevel.Professional"/>.
        /// Para administradores, será o valor definido na entidade
        /// <c>Administrator</c> (por exemplo, <c>Basic</c>, <c>Manager</c> ou <c>Super</c>).
        /// </remarks>
        public AccessLevel AccessLevel { get; set; }

        /// <summary>
        /// Token de acesso (JWT) gerado para o usuário autenticado.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Data e hora em que o token expira, em UTC.
        /// </summary>
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
