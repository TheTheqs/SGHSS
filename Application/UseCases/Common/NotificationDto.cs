// Application/UseCases/Common/NotificationDto.cs

using SGHSS.Domain.Enums;

namespace SGHSS.Application.UseCases.Common
{
    /// <summary>
    /// Representa uma notificação em formato de transferência de dados (DTO),
    /// utilizada para exibir informações resumidas ao usuário.
    /// </summary>
    public sealed class NotificationDto
    {
        /// <summary>
        /// Identificador único da notificação.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Conteúdo textual da mensagem de notificação.
        /// </summary>
        public string Message { get; init; } = string.Empty;

        /// <summary>
        /// Status atual da notificação (por exemplo: Sent, Read, etc.).
        /// </summary>
        public NotificationStatus Status { get; init; }

        /// <summary>
        /// Data e hora em que a notificação foi criada.
        /// </summary>
        public DateTimeOffset CreatedAt { get; init; }
    }
}
