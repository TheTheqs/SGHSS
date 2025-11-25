// Tests/Application/Notifications/UpdateNotificationUseCaseTests.cs

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.Notifications.Create;
using SGHSS.Application.UseCases.Notifications.Update;
using SGHSS.Domain.Enums;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Models;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Administrators;
using Xunit;

namespace SGHSS.Tests.Application.Notifications.Update
{
    public class UpdateNotificationStatusUseCaseTests
    {
        [Fact]
        public async Task Deve_Atualizar_Status_Quando_Notificacao_Existir()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            INotificationRepository notificationRepo = new NotificationRepository(context);
            IUserRepository userRepo = new UserRepository(context);
            IAdministratorRepository administratorRepo = new AdministratorRepository(context);

            // 1) Cria um usuário válido (Administrator) com consentimentos necessários
            RegisterAdministratorRequest adminExample = AdministratorGenerator.GenerateAdministrator();

            ConsentDto treatmentConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.App, true);
            ConsentDto notificationConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.App, true);

            adminExample.Consents.Add(treatmentConsent);
            adminExample.Consents.Add(notificationConsent);

            var registerAdminUseCase = new RegisterAdministratorUseCase(administratorRepo);
            var adminResult = await registerAdminUseCase.Handle(adminExample);

            adminResult.Should().NotBeNull();
            adminResult.AdministratorId.Should().NotBe(Guid.Empty);

            // 2) Cria uma notificação para esse usuário
            var createRequest = CreateNotificationRequestGenerator.Generate(
                providedRecipientId: adminResult.AdministratorId
            );

            var createUseCase = new CreateNotificationUseCase(notificationRepo, userRepo);
            var createdNotification = await createUseCase.Handle(createRequest);

            createdNotification.Should().NotBeNull();
            createdNotification.NotificationId.Should().NotBe(Guid.Empty);
            createdNotification.Status.Should().Be(NotificationStatus.Sent);

            // 3) Prepara o caso de uso de atualização de status
            var updateUseCase = new UpdateNotificationStatusUseCase(notificationRepo);

            var updateRequest = new UpdateNotificationStatusRequest
            {
                NotificationId = createdNotification.NotificationId,
                NewStatus = NotificationStatus.Read
            };

            // Act
            var updateResponse = await updateUseCase.Handle(updateRequest);

            // Assert (response)
            updateResponse.Should().NotBeNull();
            updateResponse.NotificationId.Should().Be(createdNotification.NotificationId);
            updateResponse.Status.Should().Be(NotificationStatus.Read);

            // Assert (banco)
            var persisted = await context.Notifications
                .Include(n => n.Recipient)
                .FirstAsync(n => n.Id == createdNotification.NotificationId);

            persisted.Should().NotBeNull();
            persisted.Status.Should().Be(NotificationStatus.Read);
            persisted.Recipient.Should().NotBeNull();
            persisted.Recipient.Id.Should().Be(adminResult.AdministratorId);
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Notificacao_Nao_For_Encontrada()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            INotificationRepository notificationRepo = new NotificationRepository(context);

            var updateUseCase = new UpdateNotificationStatusUseCase(notificationRepo);

            var updateRequest = new UpdateNotificationStatusRequest
            {
                NotificationId = Guid.NewGuid(), // não existe no banco
                NewStatus = NotificationStatus.Failed
            };

            // Act
            Func<Task> act = () => updateUseCase.Handle(updateRequest);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Notificação não encontrada para o identificador informado.");
        }
    }
}
