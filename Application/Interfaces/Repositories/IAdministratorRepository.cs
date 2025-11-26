// Application/Interfaces/Repositories/IAdministratorRepository.cs

using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Application.Interfaces.Repositories
{
    /// <summary>
    /// Define o contrato para operações de persistência relacionadas à entidade
    /// <see cref="Administrator"/>. Fornece métodos para verificação de existência,
    /// adição e recuperação de registros, abstraindo o acesso aos dados para a camada
    /// de aplicação.
    /// </summary>
    public interface IAdministratorRepository
    {
        /// <summary>
        /// Determina de forma assíncrona se existe um usuário com o endereço de e-mail especificado.
        /// </summary>
        /// <param name="email">O endereço de e-mail a ser verificado, encapsulado em um Value Object.</param>
        /// <returns>
        /// <see langword="true"/> se existir um administrador com o e-mail informado;
        /// caso contrário, <see langword="false"/>.
        /// </returns>
        Task<bool> ExistsByEmailAsync(Email email);

        /// <summary>
        /// Recupera um administrador pelo seu identificador único.
        /// </summary>
        /// <param name="administratorId">O identificador do administrador.</param>
        /// <returns>
        /// A entidade <see cref="Administrator"/> correspondente ao identificador informado,
        /// ou <c>null</c> caso não seja encontrada.
        /// </returns>
        /// <remarks>
        /// Este método deve ser utilizado em cenários onde a identificação do administrador
        /// é necessária para validação, auditoria ou registro de operações.
        /// </remarks>
        Task<Administrator?> GetByIdAsync(Guid administratorId);

        /// <summary>
        /// Adiciona um novo administrador ao repositório.
        /// </summary>
        /// <param name="administrator">Instância de <see cref="Administrator"/> a ser persistida.</param>
        Task AddAsync(Administrator administrator);
    }
}
