// Application/UseCases/Notifications/Update/UpdateNotificationStatusRequest.cs

using SGHSS.Domain.Enums;

namespace SGHSS.Application.UseCases.Notifications.Update
{
    /// <summary>
    /// Representa os dados de entrada necessários para atualizar o status
    /// de uma notificação existente no sistema.
    /// </summary>
    /// <remarks>
    /// Este DTO é utilizado pela camada de Application para receber a intenção
    /// de mudança de status, informando qual notificação deve ser atualizada
    /// e qual será o novo valor de <see cref="NotificationStatus"/>.
    /// </remarks>
    public sealed class UpdateNotificationStatusRequest
    {
        /// <summary>
        /// Identificador único da notificação a ser atualizada.
        /// </summary>
        public Guid NotificationId { get; init; }

        /// <summary>
        /// Novo status que será atribuído à notificação.
        /// </summary>
        public NotificationStatus NewStatus { get; init; }
    }
}
