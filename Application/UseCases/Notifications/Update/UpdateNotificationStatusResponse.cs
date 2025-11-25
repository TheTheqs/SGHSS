// Application/UseCases/Notifications/Update/UpdateNotificationStatusResponse.cs

using SGHSS.Domain.Enums;

namespace SGHSS.Application.UseCases.Notifications.Update
{
    /// <summary>
    /// Representa os dados retornados após a atualização de status
    /// de uma notificação.
    /// </summary>
    /// <remarks>
    /// Este DTO expõe as informações essenciais resultantes da operação,
    /// permitindo que a camada de interface apresente o novo estado ao cliente.
    /// </remarks>
    public sealed class UpdateNotificationStatusResponse
    {
        /// <summary>
        /// Identificador único da notificação atualizada.
        /// </summary>
        public Guid NotificationId { get; init; }

        /// <summary>
        /// Status atual da notificação após a atualização.
        /// </summary>
        public NotificationStatus Status { get; init; }
    }
}
