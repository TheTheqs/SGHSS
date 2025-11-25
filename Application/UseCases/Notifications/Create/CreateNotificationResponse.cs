// Application/UseCases/Notifications/Create/CreateNotificationResponse.cs

using SGHSS.Domain.Enums;

namespace SGHSS.Application.UseCases.Notifications.Create
{
    /// <summary>
    /// Representa os dados retornados após a criação de uma nova notificação.
    /// </summary>
    /// <remarks>
    /// Este DTO expõe as principais informações da notificação persistida,
    /// incluindo identificador, destinatário, canal, conteúdo, data de criação
    /// e status atual.
    /// </remarks>
    public sealed class CreateNotificationResponse
    {
        /// <summary>
        /// Identificador único da notificação criada.
        /// </summary>
        public Guid NotificationId { get; init; }

        /// <summary>
        /// Identificador único do usuário destinatário da notificação.
        /// </summary>
        public Guid RecipientId { get; init; }

        /// <summary>
        /// Canal utilizado para entrega da notificação.
        /// </summary>
        public NotificationChannel Channel { get; init; }

        /// <summary>
        /// Conteúdo textual da mensagem enviada ao destinatário.
        /// </summary>
        public string Message { get; init; } = string.Empty;

        /// <summary>
        /// Data e hora em que a notificação foi criada no sistema.
        /// </summary>
        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>
        /// Status atual da notificação (por padrão, será <see cref="NotificationStatus.Sent"/> após a criação).
        /// </summary>
        public NotificationStatus Status { get; init; }
    }
}
