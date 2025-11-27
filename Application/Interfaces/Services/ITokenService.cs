// Application/Interfaces/Services/ITokenService.cs

using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;

namespace SGHSS.Application.Interfaces.Services
{
    /// <summary>
    /// Define operações relacionadas à geração de tokens de autenticação.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Gera um token de acesso (por exemplo, JWT) para o usuário informado.
        /// </summary>
        /// <param name="user">Usuário autenticado.</param>
        /// <param name="accessLevel">Nível de acesso efetivo do usuário.</param>
        /// <returns>
        /// Uma tupla contendo o token gerado e a data/hora de expiração.
        /// </returns>
        (string Token, DateTimeOffset ExpiresAt) GenerateToken(User user, AccessLevel accessLevel);
    }
}
