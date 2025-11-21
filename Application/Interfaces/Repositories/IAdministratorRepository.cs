// Application/Interfaces/Repositories/IAdministratorRepository.cs

using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Application.Interfaces.Repositories
{
    /// <summary>
    /// Define o contrato para operações de persistência relacionadas à entidade <see cref="Administrator"/>.
    /// Fornece métodos para verificar a existência de registros e para adicionar novos administradores,
    /// abstraindo os detalhes de acesso a dados para a camada de aplicação.
    /// </summary>
    public interface IAdministratorRepository
    {
        /// <summary>
        /// Determina de forma assíncrona se existe um usuário com o endereço de e-mail especificado.
        /// </summary>
        /// <param name="email">O endereço de e-mail a ser verificado. Não pode ser nulo.</param>
        /// <returns>Uma tarefa que representa a operação assíncrona. O resultado da tarefa contém
        /// <see langword="true"/> se existir um usuário com o e-mail especificado; caso contrário,
        /// <see langword="false"/>.</returns>
        Task<bool> ExistsByEmailAsync(Email email);

        /// <summary>
        /// Adiciona um novo administrador ao repositório.
        /// </summary>
        /// <param name="administrator">Instância de <see cref="Administrator"/> a ser persistida.</param>
        Task AddAsync(Administrator administrator);
    }
}
