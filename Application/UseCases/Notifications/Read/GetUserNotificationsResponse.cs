// Application/UseCases/Notifications/Read/GetUserNotificationsResponse.cs

using SGHSS.Application.UseCases.Common;

namespace SGHSS.Application.UseCases.Notifications.Read
{
    /// <summary>
    /// Representa a resposta contendo a lista de notificações
    /// associadas a um usuário específico.
    /// </summary>
    public sealed class GetUserNotificationsResponse
    {
        /// <summary>
        /// Coleção de notificações do usuário.
        /// </summary>
        public IReadOnlyCollection<NotificationDto> Notifications { get; init; } =
            Array.Empty<NotificationDto>();
    }
}
