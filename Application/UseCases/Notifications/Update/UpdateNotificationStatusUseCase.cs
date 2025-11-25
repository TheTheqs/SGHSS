// Application/UseCases/Notifications/Update/UpdateNotificationStatusUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Enums;

namespace SGHSS.Application.UseCases.Notifications.Update
{
    /// <summary>
    /// Caso de uso responsável por orquestrar a atualização do status
    /// de uma notificação existente.
    /// </summary>
    /// <remarks>
    /// Este caso de uso localiza a notificação pelo identificador informado,
    /// aplica o novo status solicitado e persiste a alteração por meio
    /// do repositório de notificações.
    /// </remarks>
    public sealed class UpdateNotificationStatusUseCase
    {
        private readonly INotificationRepository _notificationRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso de atualização de status de notificação.
        /// </summary>
        /// <param name="notificationRepository">
        /// Repositório responsável por acessar e persistir entidades <see cref="Domain.Models.Notification"/>.
        /// </param>
        public UpdateNotificationStatusUseCase(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        /// <summary>
        /// Executa o fluxo de atualização de status de uma notificação.
        /// </summary>
        /// <param name="request">
        /// Objeto contendo o identificador da notificação e o novo status desejado.
        /// </param>
        /// <returns>
        /// Um <see cref="UpdateNotificationStatusResponse"/> representando
        /// o estado resultante da notificação após a operação.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Lançada quando o request é nulo.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Lançada quando o identificador da notificação é vazio.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando a notificação não é encontrada para o identificador informado.
        /// </exception>
        public async Task<UpdateNotificationStatusResponse> Handle(UpdateNotificationStatusRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request), "A requisição não pode ser nula.");
            }

            if (request.NotificationId == Guid.Empty)
            {
                throw new ArgumentException(
                    "O identificador da notificação não pode ser vazio.",
                    nameof(request.NotificationId)
                );
            }

            // Localiza a notificação no repositório
            var notification = await _notificationRepository.GetByIdAsync(request.NotificationId);

            if (notification is null)
            {
                throw new InvalidOperationException(
                    "Notificação não encontrada para o identificador informado."
                );
            }

            // Atualiza o status da notificação
            notification.Status = request.NewStatus;

            // Persiste as alterações
            await _notificationRepository.UpdateAsync(notification);

            // Monta o response com o estado final
            return new UpdateNotificationStatusResponse
            {
                NotificationId = notification.Id,
                Status = notification.Status
            };
        }
    }
}
