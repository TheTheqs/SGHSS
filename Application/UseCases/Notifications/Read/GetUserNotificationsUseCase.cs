// Application/UseCases/Notifications/Read/GetUserNotificationsUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Common;

namespace SGHSS.Application.UseCases.Notifications.Read
{
    /// <summary>
    /// Caso de uso responsável por consultar as notificações
    /// associadas a um usuário específico.
    /// </summary>
    public sealed class GetUserNotificationsUseCase
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso para consulta
        /// de notificações de usuário.
        /// </summary>
        /// <param name="notificationRepository">
        /// Repositório responsável pelo acesso às notificações.
        /// </param>
        /// <param name="userRepository">
        /// Repositório responsável pelo acesso aos dados de usuários.
        /// </param>
        public GetUserNotificationsUseCase(
            INotificationRepository notificationRepository,
            IUserRepository userRepository)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Recupera todas as notificações associadas ao usuário informado.
        /// </summary>
        /// <param name="request">
        /// Objeto contendo o identificador do usuário alvo da consulta.
        /// </param>
        /// <returns>
        /// Um <see cref="GetUserNotificationsResponse"/> contendo a lista de
        /// notificações do usuário.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Lançada quando o request fornecido é nulo.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando o identificador informado é inválido ou
        /// quando o usuário não é encontrado.
        /// </exception>
        public async Task<GetUserNotificationsResponse> Handle(GetUserNotificationsRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (request.UserId == Guid.Empty)
                throw new InvalidOperationException(
                    "É obrigatório informar um identificador de usuário válido.");

            // Garante que o usuário existe
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user is null)
                throw new InvalidOperationException(
                    "Usuário informado não foi encontrado.");

            // Recupera notificações do usuário
            var notifications = await _notificationRepository.GetByUserIdAsync(request.UserId);

            var dtos = notifications
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Message = n.Message,
                    Status = n.Status,
                    CreatedAt = n.CreatedAt
                })
                .ToArray();

            return new GetUserNotificationsResponse
            {
                Notifications = dtos
            };
        }
    }
}
