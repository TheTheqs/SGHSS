// Tests/Application/Professional/Register/RegisterProfessionalTests.cs

using FluentAssertions;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.Professionals.Register;
using SGHSS.Domain.Enums;
using SGHSS.Domain.ValueObjects;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Professionals;
using Xunit;

namespace SGHSS.Tests.Application.Professional.Register
{
    public class RegisterProfessionalTests
    {
        [Fact]
        public async Task Deve_Salvar_Novo_Profissional()
        {
            // Arrange
            RegisterProfessionalRequest example =
                ProfessionalGenerator.GenerateProfessional(durationInMinutes: 30, weeklyWindowsCount: 2);

            ConsentDto treatmentConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);

            example.Consents.Add(treatmentConsent);
            example.Consents.Add(researchConsent);
            example.Consents.Add(notificationConsent);

            // In memory database
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new ProfessionalRepository(context);

            // Act
            var useCase = new RegisterProfessionalUseCase(repo);
            var request = example;
            var result = await useCase.Handle(request);
            var existsByEmail = await repo.ExistsByEmailAsync(new Email(request.Email));

            // Assert
            result.Should().NotBeNull();
            result.ProfessionalId.Should().NotBe(Guid.Empty);
            existsByEmail.Should().BeTrue();
        }

        [Fact]
        public async Task Deve_Recusar_profissional_sem_consentimentos()
        {
            // Arrange
            RegisterProfessionalRequest example =
                ProfessionalGenerator.GenerateProfessional(durationInMinutes: 30, weeklyWindowsCount: 2);

            // Nenhum consentimento adicionado

            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new ProfessionalRepository(context);

            var useCase = new RegisterProfessionalUseCase(repo);
            var request = example;

            // Act
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("É obrigatório informar ao menos um consentimento.");

            var existsByEmail = await repo.ExistsByEmailAsync(new Email(request.Email));
            existsByEmail.Should().BeFalse();
        }

        [Fact]
        public async Task Deve_Recusar_profissional_sem_consentimento_de_tratamento()
        {
            // Arrange
            RegisterProfessionalRequest example =
                ProfessionalGenerator.GenerateProfessional(durationInMinutes: 30, weeklyWindowsCount: 2);

            ConsentDto researchConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);

            example.Consents.Add(researchConsent);
            example.Consents.Add(notificationConsent);

            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new ProfessionalRepository(context);

            var useCase = new RegisterProfessionalUseCase(repo);
            var request = example;

            // Act
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("O profissional não possui um consentimento de tratamento ativo.");

            var existsByEmail = await repo.ExistsByEmailAsync(new Email(request.Email));
            existsByEmail.Should().BeFalse();
        }

        [Fact]
        public async Task Deve_Recusar_licenca_duplicada()
        {
            // Arrange
            RegisterProfessionalRequest example =
                ProfessionalGenerator.GenerateProfessional(durationInMinutes: 30, weeklyWindowsCount: 2);

            ConsentDto treatmentConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);

            example.Consents.Add(treatmentConsent);
            example.Consents.Add(researchConsent);
            example.Consents.Add(notificationConsent);

            // Segundo profissional com mesma licença
            RegisterProfessionalRequest example2 =
                ProfessionalGenerator.GenerateProfessional(
                    durationInMinutes: 30,
                    weeklyWindowsCount: 2,
                    providedLicense: example.License);

            ConsentDto treatmentConsent2 =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsent2 =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsent2 =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);

            example2.Consents.Add(treatmentConsent2);
            example2.Consents.Add(researchConsent2);
            example2.Consents.Add(notificationConsent2);

            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new ProfessionalRepository(context);

            var useCase = new RegisterProfessionalUseCase(repo);

            // Act 1: deve salvar o primeiro
            var request = example;
            var result = await useCase.Handle(request);

            result.Should().NotBeNull();
            result.ProfessionalId.Should().NotBe(Guid.Empty);

            var existsLicense = await repo.ExistsByProfessionalLicenseAsync(new ProfessionalLicense(request.License));
            existsLicense.Should().BeTrue();

            // Act 2: tentar salvar segundo profissional com mesma licença
            var request2 = example2;
            Func<Task> act = () => useCase.Handle(request2);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Já existe um profissional cadastrado com a licença informada.");
        }

        [Fact]
        public async Task Deve_Recusar_email_duplicado()
        {
            // Arrange
            RegisterProfessionalRequest example =
                ProfessionalGenerator.GenerateProfessional(durationInMinutes: 30, weeklyWindowsCount: 2);

            ConsentDto treatmentConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);

            example.Consents.Add(treatmentConsent);
            example.Consents.Add(researchConsent);
            example.Consents.Add(notificationConsent);

            // Segundo profissional com o MESMO email
            RegisterProfessionalRequest example2 =
                ProfessionalGenerator.GenerateProfessional(
                    durationInMinutes: 30,
                    weeklyWindowsCount: 2,
                    providedEmail: example.Email);

            ConsentDto treatmentConsent2 =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsent2 =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsent2 =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);

            example2.Consents.Add(treatmentConsent2);
            example2.Consents.Add(researchConsent2);
            example2.Consents.Add(notificationConsent2);

            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new ProfessionalRepository(context);

            var useCase = new RegisterProfessionalUseCase(repo);

            // Act 1: salva primeiro profissional
            var request = example;
            var result = await useCase.Handle(request);

            result.Should().NotBeNull();
            result.ProfessionalId.Should().NotBe(Guid.Empty);

            var existsEmail = await repo.ExistsByEmailAsync(new Email(request.Email));
            existsEmail.Should().BeTrue();

            // Act 2: tenta salvar segundo com mesmo email
            var request2 = example2;
            Func<Task> act = () => useCase.Handle(request2);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Já existe um profissional cadastrado com o Email informado.");
        }
    }
}
