// Tests/Application/Notifications/CreateNotificationUseCaseTests.cs

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.Notifications.Create;
using SGHSS.Domain.Enums;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Models;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Administrators;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Common;
using Xunit;

namespace SGHSS.Tests.Application.Notifications.Create
{
    public class CreateNotificationUseCaseTests
    {
        [Fact]
        public async Task Deve_Criar_Notificacao_Quando_Dados_Forem_Validos()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            INotificationRepository notificationRepo = new NotificationRepository(context);
            IUserRepository userRepo = new UserRepository(context);
            IAdministratorRepository administratorRepo = new AdministratorRepository(context);

            // 1) Cria um usuário válido (vamos usar um Administrator como destinatário)
            RegisterAdministratorRequest adminExample = AdministratorGenerator.GenerateAdministrator();
            ConsentDto treatmentConsent = ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.App, true);
            ConsentDto notificationConsent = ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.App, true);
            adminExample.Consents.Add(treatmentConsent);
            adminExample.Consents.Add(notificationConsent);

            var registerAdminUseCase = new RegisterAdministratorUseCase(administratorRepo);
            var adminResult = await registerAdminUseCase.Handle(adminExample);

            adminResult.Should().NotBeNull();
            adminResult.AdministratorId.Should().NotBe(Guid.Empty);

            // 2) Gera o request de notificação apontando para esse usuário
            var request = CreateNotificationRequestGenerator.Generate(
                providedRecipientId: adminResult.AdministratorId
            );

            var useCase = new CreateNotificationUseCase(notificationRepo, userRepo);

            // Act
            var response = await useCase.Handle(request);

            // Assert (response)
            response.Should().NotBeNull();
            response.NotificationId.Should().NotBe(Guid.Empty);
            response.RecipientId.Should().Be(adminResult.AdministratorId);
            response.Channel.Should().Be(request.Channel);
            response.Message.Should().Be(request.Message);
            response.Status.Should().Be(NotificationStatus.Sent);
            response.CreatedAt.Should().NotBe(default);

            // Assert (banco)
            var persistedNotification = await context.Notifications
                .Include(n => n.Recipient)
                .FirstAsync(n => n.Id == response.NotificationId);

            persistedNotification.Should().NotBeNull();
            persistedNotification.Recipient.Should().NotBeNull();
            persistedNotification.Recipient.Id.Should().Be(adminResult.AdministratorId);
            persistedNotification.Channel.Should().Be(request.Channel);
            persistedNotification.Message.Should().Be(request.Message);
            persistedNotification.Status.Should().Be(NotificationStatus.Sent);

            // CreatedAt do response deve estar bem próximo do valor persistido
            response.CreatedAt.Should().BeCloseTo(
                persistedNotification.CreatedAt,
                precision: TimeSpan.FromSeconds(5)
            );
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Usuario_Nao_For_Encontrado()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            INotificationRepository notificationRepo = new NotificationRepository(context);
            IUserRepository userRepo = new UserRepository(context);

            var useCase = new CreateNotificationUseCase(notificationRepo, userRepo);

            // Gera um request completamente válido, porém com RecipientId que não existe no banco
            var request = CreateNotificationRequestGenerator.Generate();

            // Act
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Usuário não encontrado para o identificador informado.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Mensagem_Estiver_Vazia()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            INotificationRepository notificationRepo = new NotificationRepository(context);
            IUserRepository userRepo = new UserRepository(context);
            IAdministratorRepository administratorRepo = new AdministratorRepository(context);

            // Cria um usuário válido (Administrator) com consentimentos necessários
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

            var useCase = new CreateNotificationUseCase(notificationRepo, userRepo);

            // Request montado manualmente com mensagem só de espaços
            var request = new CreateNotificationRequest
            {
                RecipientId = adminResult.AdministratorId,
                Channel = NotificationChannel.Email,
                Message = " "
            };

            // Act
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("A mensagem da notificação não pode ser vazia.");
        }
    }
}
