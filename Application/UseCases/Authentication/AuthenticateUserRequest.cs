// Application/UseCases/Authentication/AuthenticateUserRequest.cs

namespace SGHSS.Application.UseCases.Authentication
{
    /// <summary>
    /// Representa os dados necessários para autenticar um usuário no sistema.
    /// </summary>
    /// <remarks>
    /// Este request é utilizado pelo caso de uso de autenticação para validar
    /// as credenciais informadas (e-mail e senha) e, em caso de sucesso,
    /// gerar o token JWT correspondente.
    /// </remarks>
    public class AuthenticateUserRequest
    {
        /// <summary>
        /// Endereço de e-mail utilizado para autenticação.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Senha em texto puro informada pelo usuário. 
        /// Será convertida e validada contra o value object <c>Password</c>.
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}
