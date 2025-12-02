// Application/Interfaces/Repositories/INotificationRepository.cs

using SGHSS.Domain.Models;

namespace SGHSS.Application.Interfaces.Repositories
{
    /// <summary>
    /// Define operações de acesso a dados relacionadas a notificações do sistema.
    /// </summary>
    public interface INotificationRepository
    {
        /// <summary>
        /// Persiste uma nova notificação no banco de dados.
        /// </summary>
        /// <param name="notification">Instância de <see cref="Notification"/> a ser salva.</param>
        Task AddAsync(Notification notification);

        /// <summary>
        /// Atualiza uma notificação existente no banco de dados.
        /// </summary>
        /// <param name="notification">
        /// Entidade <see cref="Notification"/> com os dados já modificados e pronta
        /// para ser persistida.
        /// </param>
        Task UpdateAsync(Notification notification);

        /// <summary>
        /// Recupera uma notificação pelo seu identificador único.
        /// </summary>
        /// <param name="notificationId">Identificador da notificação a ser localizada.</param>
        /// <returns>
        /// A entidade <see cref="Notification"/> correspondente ao ID informado,
        /// ou <c>null</c> caso nenhum registro seja encontrado.
        /// </returns>
        Task<Notification?> GetByIdAsync(Guid notificationId);

        /// <summary>
        /// Retorna todas as notificações pertencentes ao usuário informado.
        /// </summary>
        /// <param name="userId">Identificador do usuário destinatário.</param>
        /// <returns>Coleção de notificações associadas ao usuário.</returns>
        Task<IReadOnlyCollection<Notification>> GetByUserIdAsync(Guid userId);
    }
}
