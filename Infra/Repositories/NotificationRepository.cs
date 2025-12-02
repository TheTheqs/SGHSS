// Infra/Repositories/NotificationRepository.cs

using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Models;
using SGHSS.Infra.Persistence;

namespace SGHSS.Infra.Repositories
{
    /// <summary>
    /// Repositório responsável por realizar operações de persistência relacionadas à entidade
    /// <see cref="Notification"/> utilizando o Entity Framework Core.
    /// </summary>
    /// <remarks>
    /// Esta implementação segue o padrão de repositório e fornece uma abstração sobre o acesso ao banco,
    /// permitindo que a camada de aplicação trabalhe sem dependências diretas do EF Core.
    /// </remarks>
    public class NotificationRepository : INotificationRepository
    {
        private readonly SGHSSDbContext _context;

        /// <summary>
        /// Cria uma nova instância do repositório de notificações.
        /// </summary>
        /// <param name="context">O contexto de banco de dados utilizado para acesso aos dados.</param>
        public NotificationRepository(SGHSSDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Persiste uma nova notificação no banco de dados.
        /// </summary>
        /// <param name="notification">Instância de <see cref="Notification"/> a ser salva.</param>
        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Atualiza uma notificação existente no banco de dados.
        /// </summary>
        /// <param name="notification">
        /// Entidade <see cref="Notification"/> com os dados já modificados e pronta
        /// para ser persistida.
        /// </param>
        public async Task UpdateAsync(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Recupera uma notificação pelo seu identificador único.
        /// </summary>
        /// <param name="notificationId">Identificador da notificação a ser localizada.</param>
        /// <returns>
        /// A entidade <see cref="Notification"/> correspondente ao ID informado,
        /// ou <c>null</c> caso nenhum registro seja encontrado.
        /// </returns>
        /// <remarks>
        /// Inclui o carregamento do destinatário associado à notificação.
        /// </remarks>
        public async Task<Notification?> GetByIdAsync(Guid notificationId)
        {
            return await _context.Notifications
                .Include(n => n.Recipient)
                .FirstOrDefaultAsync(n => n.Id == notificationId);
        }

        /// <summary>
        /// Retorna todas as notificações pertencentes ao usuário informado.
        /// </summary>
        /// <param name="userId">Identificador do usuário destinatário.</param>
        /// <returns>Coleção de notificações associadas ao usuário.</returns>
        public async Task<IReadOnlyCollection<Notification>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(n => n.Recipient.Id == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }
    }
}
