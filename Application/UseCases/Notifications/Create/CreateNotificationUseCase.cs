// Application/UseCases/Notifications/Create/CreateNotificationUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;

namespace SGHSS.Application.UseCases.Notifications.Create
{
    /// <summary>
    /// Caso de uso responsável por orquestrar a criação de uma nova notificação
    /// destinada a um usuário do sistema.
    /// </summary>
    /// <remarks>
    /// Este caso de uso aplica validações básicas sobre os dados de entrada,
    /// garante a existência do destinatário e persiste a notificação configurando
    /// o status inicial como <see cref="NotificationStatus.Sent"/>.
    /// </remarks>
    public sealed class CreateNotificationUseCase
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso de criação de notificação.
        /// </summary>
        /// <param name="notificationRepository">
        /// Repositório responsável por persistir e consultar notificações.
        /// </param>
        /// <param name="userRepository">
        /// Repositório responsável por acessar os dados dos usuários destinatários.
        /// </param>
        public CreateNotificationUseCase(
            INotificationRepository notificationRepository,
            IUserRepository userRepository)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Executa o fluxo de criação de uma nova notificação.
        /// </summary>
        /// <param name="request">
        /// Objeto contendo o identificador do destinatário, o canal e a mensagem da notificação.
        /// </param>
        /// <returns>
        /// Um <see cref="CreateNotificationResponse"/> com os dados da notificação criada
        /// e persistida no sistema.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Lançada quando o request é nulo.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Lançada quando o identificador do destinatário é vazio.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando a mensagem é vazia ou quando o destinatário não é encontrado.
        /// </exception>
        public async Task<CreateNotificationResponse> Handle(CreateNotificationRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request), "A requisição não pode ser nula.");
            }

            if (request.RecipientId == Guid.Empty)
            {
                throw new ArgumentException(
                    "O identificador do destinatário não pode ser vazio.",
                    nameof(request.RecipientId)
                );
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                throw new InvalidOperationException(
                    "A mensagem da notificação não pode ser vazia."
                );
            }

            // Busca o destinatário no repositório de usuários.
            var recipient = await _userRepository.GetByIdAsync(request.RecipientId);

            if (recipient is null)
            {
                throw new InvalidOperationException(
                    "Usuário não encontrado para o identificador informado."
                );
            }

            // Cria a entidade de domínio Notification.
            // Observação: o Id será gerado pela infraestrutura/EF, conforme configuração.
            var notification = new Notification
            {
                Channel = request.Channel,
                Message = request.Message,
                CreatedAt = DateTimeOffset.UtcNow,
                Status = NotificationStatus.Sent,
                Recipient = recipient
            };

            // Persistência da notificação
            await _notificationRepository.AddAsync(notification);

            // Monta o response com os dados persistidos
            return new CreateNotificationResponse
            {
                NotificationId = notification.Id,
                RecipientId = recipient.Id,
                Channel = notification.Channel,
                Message = notification.Message,
                CreatedAt = notification.CreatedAt,
                Status = notification.Status
            };
        }
    }
}
