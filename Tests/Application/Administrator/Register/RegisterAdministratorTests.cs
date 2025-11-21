// Tests/Application/Administrator/Register/RegisterAdministratorTests.cs

using FluentAssertions;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Domain.Enums;
using SGHSS.Domain.ValueObjects;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Administrators;
using Xunit;

namespace SGHSS.Tests.Application.Administrator.Register
{
    public class RegisterAdministratorTests
    {

        [Fact]
        public async Task Deve_Salvar_Novo_Administrador()
        {
            // Arrange
            RegisterAdministratorRequest _example = AdministratorGenerator.GenerateAdministrator();
            ConsentDto treatmentConsent = ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsent = ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsent = ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);
            _example.Consents.Add(treatmentConsent);
            _example.Consents.Add(researchConsent);
            _example.Consents.Add(notificationConsent);

            // In memory database
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new AdministratorRepository(context);

            // Act
            var useCase = new RegisterAdministratorUseCase(repo);
            var request = _example;
            var result = await useCase.Handle(request);
            var exists = await repo.ExistsByEmailAsync(new Email(request.Email));

            // Assert
            result.Should().NotBeNull();
            result.AdministratorId.Should().NotBe(Guid.Empty);
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task Deve_recusar_administrador_sem_consentimentos()
        {
            // Arrange
            RegisterAdministratorRequest _example = AdministratorGenerator.GenerateAdministrator();

            // In memory database
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new AdministratorRepository(context);

            // Act
            var useCase = new RegisterAdministratorUseCase(repo);
            var request = _example;
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("É obrigatório informar ao menos um consentimento.");

            var exists = await repo.ExistsByEmailAsync(new Email(request.Email));
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task Deve_recusar_administrador_sem_consentimento_de_tratamento()
        {
            // Arrange
            RegisterAdministratorRequest _example = AdministratorGenerator.GenerateAdministrator();
            ConsentDto researchConsent = ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsent = ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);
            _example.Consents.Add(researchConsent);
            _example.Consents.Add(notificationConsent);

            // In memory database
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new AdministratorRepository(context);

            // Act
            var useCase = new RegisterAdministratorUseCase(repo);
            var request = _example;
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("O administrador não possui um consentimento de tratamento ativo.");

            var exists = await repo.ExistsByEmailAsync(new Email(request.Email));
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task Deve_Recusar_email_duplicado()
        {
            // Arrange
            RegisterAdministratorRequest _example = AdministratorGenerator.GenerateAdministrator();
            ConsentDto treatmentConsent = ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsent = ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsent = ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);
            _example.Consents.Add(treatmentConsent);
            _example.Consents.Add(researchConsent);
            _example.Consents.Add(notificationConsent);

            RegisterAdministratorRequest _example2 = AdministratorGenerator.GenerateAdministrator(providedEmail: _example.Email);
            ConsentDto treatmentConsent2 = ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsent2 = ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsent2 = ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);
            _example2.Consents.Add(treatmentConsent2);
            _example2.Consents.Add(researchConsent2);
            _example2.Consents.Add(notificationConsent2);

            // In memory database
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new AdministratorRepository(context);

            // Act 1
            var useCase = new RegisterAdministratorUseCase(repo);
            var request = _example;
            var result = await useCase.Handle(request);
            var exists = await repo.ExistsByEmailAsync(new Email(request.Email));
            result.Should().NotBeNull();
            result.AdministratorId.Should().NotBe(Guid.Empty);
            exists.Should().BeTrue();

            // Act 2
            var request2 = _example2;
            Func<Task> act = () => useCase.Handle(request2);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Já existe um administrador cadastrado com o e-mail informado.");
        }
    }
}
