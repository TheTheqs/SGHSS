// Application/UseCases/Notifications/Read/GetUserNotificationsRequest.cs

namespace SGHSS.Application.UseCases.Notifications.Read
{
    /// <summary>
    /// Representa a solicitação para consulta das notificações
    /// associadas a um determinado usuário.
    /// </summary>
    public sealed class GetUserNotificationsRequest
    {
        /// <summary>
        /// Identificador único do usuário cujas notificações
        /// devem ser recuperadas.
        /// </summary>
        public Guid UserId { get; init; }
    }
}
