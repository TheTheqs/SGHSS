// Application/UseCases/Notifications/Create/CreateNotificationRequest.cs

using SGHSS.Domain.Enums;

namespace SGHSS.Application.UseCases.Notifications.Create
{
    /// <summary>
    /// Representa os dados de entrada necessários para criar uma nova notificação
    /// destinada a um usuário específico.
    /// </summary>
    /// <remarks>
    /// Este DTO é utilizado pela camada de Application para receber as informações
    /// vindas da camada de interface (por exemplo, uma API), encapsulando o
    /// identificador do destinatário, o canal de entrega e o conteúdo da mensagem.
    /// </remarks>
    public sealed class CreateNotificationRequest
    {
        /// <summary>
        /// Identificador único do usuário que receberá a notificação.
        /// </summary>
        public Guid RecipientId { get; init; }

        /// <summary>
        /// Canal de entrega da notificação (por exemplo: Email, SMS ou PushNotification).
        /// </summary>
        public NotificationChannel Channel { get; init; }

        /// <summary>
        /// Conteúdo textual da mensagem de notificação.
        /// </summary>
        public string Message { get; init; } = string.Empty;
    }
}
