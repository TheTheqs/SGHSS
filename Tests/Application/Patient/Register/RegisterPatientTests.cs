// Tests/Application/Patient/Register/RegisterPatientTests.cs


using FluentAssertions;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.Patients.Register;
using SGHSS.Domain.Enums;
using SGHSS.Domain.ValueObjects;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Patients;
using Xunit;

namespace SGHSS.Tests.Application.Patient.Register
{
    public class RegisterPatientTests
    {

        [Fact]
        public async Task Deve_Salvar_Novo_Paciente()
        {
            // Arrange
            RegisterPatientRequest _example = PatientGenerator.GeneratePatient();
            ConsentDto treatmentConsent = ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsent = ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsent = ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);
            _example.Consents.Add(treatmentConsent);
            _example.Consents.Add(researchConsent);
            _example.Consents.Add(notificationConsent);

            // In memory database
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new PatientRepository(context);

            // Act
            var useCase = new RegisterPatientUseCase(repo);
            var request = _example;        
            var result = await useCase.Handle(request);
            var consult = await repo.ExistsByCpfAsync(new Cpf(request.Cpf));

            result.Should().NotBeNull();
            result.PatientId.Should().NotBe(Guid.Empty);
            consult.Should().BeTrue();
        }

        [Fact]
        public async Task Deve_Recusar_Novo_Paciente_Menor_de_Idade()
        {
            // Arrange
            RegisterPatientRequest _example = PatientGenerator.GeneratePatient(isUnderage: true);
            ConsentDto treatmentConsent = ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsent = ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsent = ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);
            _example.Consents.Add(treatmentConsent);
            _example.Consents.Add(researchConsent);
            _example.Consents.Add(notificationConsent);

            // In memory database
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new PatientRepository(context);

            // Act
            var useCase = new RegisterPatientUseCase(repo);
            var request = _example;
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("O paciente deve ser maior de idade.");

            var exists = await repo.ExistsByCpfAsync(new Cpf(request.Cpf));
            exists.Should().BeFalse();

        }

        [Fact]
        public async Task Deve_recusar_paciente_sem_consentimentos()
        {
            // Arrange
            RegisterPatientRequest _example = PatientGenerator.GeneratePatient();

            // In memory database
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new PatientRepository(context);

            // Act
            var useCase = new RegisterPatientUseCase(repo);
            var request = _example;
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("É obrigatório informar ao menos um consentimento.");

            var exists = await repo.ExistsByCpfAsync(new Cpf(request.Cpf));
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task Deve_recusar_paciente_sem_consentimento_de_tratamento()
        {
            // Arrange
            RegisterPatientRequest _example = PatientGenerator.GeneratePatient();
            ConsentDto researchConsent = ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsent = ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);
            _example.Consents.Add(researchConsent);
            _example.Consents.Add(notificationConsent);

            // In memory database
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new PatientRepository(context);

            // Act
            var useCase = new RegisterPatientUseCase(repo);
            var request = _example;
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("O paciente não possui um consentimento de tratamento ativo.");

            var exists = await repo.ExistsByCpfAsync(new Cpf(request.Cpf));
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task Deve_Recusar_cpf_duplicado()
        {
            // Arrange
            RegisterPatientRequest _example = PatientGenerator.GeneratePatient();
            ConsentDto treatmentConsent = ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsent = ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsent = ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);
            _example.Consents.Add(treatmentConsent);
            _example.Consents.Add(researchConsent);
            _example.Consents.Add(notificationConsent);

            RegisterPatientRequest _example2 = PatientGenerator.GeneratePatient(providedCpf: _example.Cpf);
            ConsentDto treatmentConsent2 = ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsent2 = ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsent2 = ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);
            _example.Consents.Add(treatmentConsent2);
            _example.Consents.Add(researchConsent2);
            _example.Consents.Add(notificationConsent2);

            // In memory database
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new PatientRepository(context);

            // Act 1
            var useCase = new RegisterPatientUseCase(repo);
            var request = _example;
            var result = await useCase.Handle(request);
            var consult = await repo.ExistsByCpfAsync(new Cpf(request.Cpf));
            result.Should().NotBeNull();
            result.PatientId.Should().NotBe(Guid.Empty);
            consult.Should().BeTrue();

            // Act 2
            var request2 = _example2;
            Func<Task> act = () => useCase.Handle(request2);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Já existe um paciente cadastrado com o CPF informado.");

        }

        [Fact]
        public async Task Deve_Recusar_email_duplicado()
        {
            // Arrange
            RegisterPatientRequest _example = PatientGenerator.GeneratePatient();
            ConsentDto treatmentConsent = ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsent = ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsent = ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);
            _example.Consents.Add(treatmentConsent);
            _example.Consents.Add(researchConsent);
            _example.Consents.Add(notificationConsent);

            RegisterPatientRequest _example2 = PatientGenerator.GeneratePatient(providedEmail: _example.Email);
            ConsentDto treatmentConsent2 = ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsent2 = ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsent2 = ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);
            _example.Consents.Add(treatmentConsent2);
            _example.Consents.Add(researchConsent2);
            _example.Consents.Add(notificationConsent2);

            // In memory database
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new PatientRepository(context);

            // Act 1
            var useCase = new RegisterPatientUseCase(repo);
            var request = _example;
            var result = await useCase.Handle(request);
            var consult = await repo.ExistsByCpfAsync(new Cpf(request.Cpf));
            result.Should().NotBeNull();
            result.PatientId.Should().NotBe(Guid.Empty);
            consult.Should().BeTrue();

            // Act 2
            var request2 = _example2;
            Func<Task> act = () => useCase.Handle(request2);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Já existe um paciente cadastrado com o Email informado.");
        }
    }

}