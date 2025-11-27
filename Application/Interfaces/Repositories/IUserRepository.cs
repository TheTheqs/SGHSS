// Application/Interfaces/Repositories/IUserRepository.cs

using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Application.Interfaces.Repositories
{
    /// <summary>
    /// Define operações de acesso a dados relacionadas a usuários do sistema.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Recupera um usuário pelo seu identificador único.
        /// </summary>
        /// <param name="userId">Identificador do usuário a ser localizado.</param>
        /// <returns>
        /// A entidade <see cref="User"/> correspondente ao ID informado,
        /// ou <c>null</c> caso nenhum usuário seja encontrado.
        /// </returns>
        Task<User?> GetByIdAsync(Guid userId);

        /// <summary>
        /// Recupera um usuário pelo seu endereço de e-mail.
        /// </summary>
        /// <param name="email">
        /// Endereço de e-mail encapsulado no value object <see cref="Email"/>.
        /// </param>
        /// <returns>
        /// Uma instância concreta de <see cref="User"/> (por exemplo,
        /// <see cref="Patient"/>, <see cref="Professional"/> ou <see cref="Administrator"/>),
        /// ou <c>null</c> caso nenhum usuário seja encontrado com o e-mail informado.
        /// </returns>
        Task<User?> GetByEmailAsync(Email email);
    }
}
