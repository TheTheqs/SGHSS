// Application/Interfaces/Repositories/IUserRepository.cs

using SGHSS.Domain.Models;

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
    }
}
