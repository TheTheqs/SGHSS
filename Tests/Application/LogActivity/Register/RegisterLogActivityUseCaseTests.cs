// Tests/Application/LogActivities/Register/RegisterLogActivityUseCaseTests.cs

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.LogActivities.Register;
using SGHSS.Application.UseCases.Common;
using SGHSS.Domain.Enums;
using SGHSS.Domain.ValueObjects;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Models;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Administrators;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Common;
using Xunit;

namespace SGHSS.Tests.Application.LogActivities.Register
{
    public class RegisterLogActivityUseCaseTests
    {
        [Fact]
        public async Task Deve_Registrar_Log_Quando_Dados_Forem_Validos()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            ILogActivityRepository logRepo = new LogActivityRepository(context);
            IUserRepository userRepo = new UserRepository(context);
            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IAdministratorRepository administratorRepo = new AdministratorRepository(context);

            // ===== Cria HealthUnit válida =====
            var healthUnitRequest = HealthUnitGenerator.GenerateHealthUnit();
            var registerHealthUnitUseCase = new RegisterHealthUnitUseCase(healthUnitRepo);
            var healthUnitResult = await registerHealthUnitUseCase.Handle(healthUnitRequest);

            healthUnitResult.Should().NotBeNull();
            healthUnitResult.HealthUnitId.Should().NotBe(Guid.Empty);

            Guid healthUnitId = healthUnitResult.HealthUnitId;

            // ===== Cria um usuário válido (Administrator) com consentimentos =====
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

            Guid userId = adminResult.AdministratorId;

            // ===== Monta request de LogActivity =====
            var request = new RegisterLogActivityRequest
            {
                UserId = userId,
                HealthUnitId = healthUnitId,
                Action = "RegisterHomeCare",
                Description = "HomeCare registered successfully.",
                IpAddress = new IpAddress("192.168.0.10"),
                Result = LogResult.Success
            };

            var useCase = new RegisterLogActivityUseCase(
                logRepo,
                userRepo,
                healthUnitRepo);

            // Act
            var response = await useCase.Handle(request);

            // Assert (response)
            response.Should().NotBeNull();
            response.LogActivityId.Should().NotBe(Guid.Empty);

            // Assert (banco)
            var persistedLog = await context.LogActivities
                .Include(l => l.User)
                .Include(l => l.HealthUnit)
                .FirstAsync(l => l.Id == response.LogActivityId);

            persistedLog.Should().NotBeNull();
            persistedLog.User.Should().NotBeNull();
            persistedLog.User.Id.Should().Be(userId);

            persistedLog.HealthUnit.Should().NotBeNull();
            persistedLog.HealthUnit.Id.Should().Be(healthUnitId);

            persistedLog.Action.Should().Be(request.Action);
            persistedLog.Description.Should().Be(request.Description);
            persistedLog.Result.Should().Be(request.Result);
            persistedLog.IpAddress.Value.Should().Be(request.IpAddress.Value);

            persistedLog.Timestamp.Should().NotBe(default);
            persistedLog.Timestamp.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-2));
        }


        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Acao_For_Vazia()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            ILogActivityRepository logRepo = new LogActivityRepository(context);
            IUserRepository userRepo = new UserRepository(context);
            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IAdministratorRepository administratorRepo = new AdministratorRepository(context);

            // ===== Cria Administrator válido =====
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

            Guid userId = adminResult.AdministratorId;

            var useCase = new RegisterLogActivityUseCase(
                logRepo,
                userRepo,
                healthUnitRepo);

            // Request com Action apenas com espaços
            var request = new RegisterLogActivityRequest
            {
                UserId = userId,
                HealthUnitId = null,
                Action = "   ",
                Description = "This description will not matter because the action is invalid.",
                IpAddress = new IpAddress("127.0.0.1"),
                Result = LogResult.Warning
            };

            // Act
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("A ação registrada no log não pode ser vazia.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Request_For_Nulo()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            ILogActivityRepository logRepo = new LogActivityRepository(context);
            IUserRepository userRepo = new UserRepository(context);
            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);

            var useCase = new RegisterLogActivityUseCase(
                logRepo,
                userRepo,
                healthUnitRepo);

            // Act
            Func<Task> act = () => useCase.Handle(null!);

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentNullException>()
                .WithMessage("A requisição não pode ser nula.*");
        }
    }
}
