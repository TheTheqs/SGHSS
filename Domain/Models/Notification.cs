// Domain/Models/Notification.cs

using SGHSS.Domain.Enums;

namespace SGHSS.Domain.Models
{
    /// <summary>
    /// Representa uma notificação do sistema enviada para um destinatário específico, incluindo o canal de entrega,
    /// o conteúdo da mensagem, o timestamp de criação e o status atual.
    /// </summary>
    /// <remarks>Uma notificação serve como uma mensagem informativa gerada pelo sistema, normalmente usada
    /// para comunicar atualizações, alertas ou ações necessárias a um usuário. Esta classe encapsula os dados
    /// essenciais e os relacionamentos necessários para rastrear e gerenciar notificações dentro do sistema de saúde.</remarks>

    public class Notification
    {
        public Guid Id { get; set; }
        public NotificationChannel Channel { get; set; } // Enum
        public string Message { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; }
        public NotificationStatus Status { get; set; } // Enum

        // Relacionamentos
        public User Recipient { get; set; } = null!;

        // Construtor padrão
        public Notification() { }
    }
}
