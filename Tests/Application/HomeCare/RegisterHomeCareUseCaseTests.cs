// Tests/Application/HomeCares/RegisterHomeCareUseCaseTests.cs

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.HomeCares.Register;
using SGHSS.Application.UseCases.Patients.Register;
using SGHSS.Application.UseCases.Professionals.Register;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Models;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Patients;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Professionals;
using Xunit;

namespace SGHSS.Tests.Application.HomeCares.Register
{
    public class RegisterHomeCareUseCaseTests
    {
        [Fact]
        public async Task Deve_Registrar_HomeCare_Quando_Dados_Forem_Validos()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IHomeCareRepository homeCareRepo = new HomeCareRepository(context);
            var patientRepo = new PatientRepository(context);
            var professionalRepo = new ProfessionalRepository(context);
            var healthUnitRepo = new HealthUnitRepository(context);

            // ===== Cria HealthUnit válida =====
            var healthUnitRequest = HealthUnitGenerator.GenerateHealthUnit();
            var registerHealthUnitUseCase = new RegisterHealthUnitUseCase(healthUnitRepo);
            var healthUnitResult = await registerHealthUnitUseCase.Handle(healthUnitRequest);

            healthUnitResult.Should().NotBeNull();
            healthUnitResult.HealthUnitId.Should().NotBe(Guid.Empty);

            Guid healthUnitId = healthUnitResult.HealthUnitId;

            // ===== Cria Professional válido =====
            RegisterProfessionalRequest professionalExample =
                ProfessionalGenerator.GenerateProfessional(durationInMinutes: 30, weeklyWindowsCount: 0);

            ConsentDto treatmentConsentProf =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsentProf =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsentProf =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);

            professionalExample.Consents.Add(treatmentConsentProf);
            professionalExample.Consents.Add(researchConsentProf);
            professionalExample.Consents.Add(notificationConsentProf);

            var registerProfessionalUseCase = new RegisterProfessionalUseCase(professionalRepo);
            var professionalResult = await registerProfessionalUseCase.Handle(professionalExample);

            professionalResult.Should().NotBeNull();
            professionalResult.ProfessionalId.Should().NotBe(Guid.Empty);

            Guid professionalId = professionalResult.ProfessionalId;

            // ===== Cria Patient válido =====
            RegisterPatientRequest patientExample =
                PatientGenerator.GeneratePatient(isUnderage: false);

            ConsentDto treatmentConsentPatient =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsentPatient =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsentPatient =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);

            patientExample.Consents.Add(treatmentConsentPatient);
            patientExample.Consents.Add(researchConsentPatient);
            patientExample.Consents.Add(notificationConsentPatient);

            var registerPatientUseCase = new RegisterPatientUseCase(patientRepo);
            var patientResult = await registerPatientUseCase.Handle(patientExample);

            patientResult.Should().NotBeNull();
            patientResult.PatientId.Should().NotBe(Guid.Empty);

            Guid patientId = patientResult.PatientId;

            // ===== Monta request de HomeCare usando generator =====
            var request = RegisterHomeCareRequestGenerator.Generate(
                patientId: patientId,
                professionalId: professionalId,
                healthUnitId: healthUnitId
            );

            var useCase = new RegisterHomeCareUseCase(
                homeCareRepo,
                patientRepo,
                professionalRepo,
                healthUnitRepo);

            // Act
            var response = await useCase.Handle(request);

            // Assert (response)
            response.Should().NotBeNull();
            response.HomeCareId.Should().NotBe(Guid.Empty);

            // Assert (banco)
            var persistedHomeCare = await context.HomeCares
                .Include(hc => hc.Patient)
                .Include(hc => hc.Professional)
                .Include(hc => hc.HealthUnit)
                .FirstAsync(hc => hc.Id == response.HomeCareId);

            persistedHomeCare.Should().NotBeNull();
            persistedHomeCare.Patient.Should().NotBeNull();
            persistedHomeCare.Professional.Should().NotBeNull();
            persistedHomeCare.HealthUnit.Should().NotBeNull();

            persistedHomeCare.Patient.Id.Should().Be(patientId);
            persistedHomeCare.Professional.Id.Should().Be(professionalId);
            persistedHomeCare.HealthUnit.Id.Should().Be(healthUnitId);

            persistedHomeCare.Description.Should().Be(request.Description);
            persistedHomeCare.Date.Should().BeCloseTo(request.Date, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Paciente_Nao_For_Encontrado()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IHomeCareRepository homeCareRepo = new HomeCareRepository(context);
            var patientRepo = new PatientRepository(context);
            var professionalRepo = new ProfessionalRepository(context);
            var healthUnitRepo = new HealthUnitRepository(context);

            // ===== Cria HealthUnit válida =====
            var healthUnitRequest = HealthUnitGenerator.GenerateHealthUnit();
            var registerHealthUnitUseCase = new RegisterHealthUnitUseCase(healthUnitRepo);
            var healthUnitResult = await registerHealthUnitUseCase.Handle(healthUnitRequest);
            Guid healthUnitId = healthUnitResult.HealthUnitId;

            // ===== Cria Professional válido =====
            RegisterProfessionalRequest professionalExample =
                ProfessionalGenerator.GenerateProfessional(durationInMinutes: 30, weeklyWindowsCount: 0);

            ConsentDto treatmentConsentProf =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsentProf =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsentProf =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);

            professionalExample.Consents.Add(treatmentConsentProf);
            professionalExample.Consents.Add(researchConsentProf);
            professionalExample.Consents.Add(notificationConsentProf);

            var registerProfessionalUseCase = new RegisterProfessionalUseCase(professionalRepo);
            var professionalResult = await registerProfessionalUseCase.Handle(professionalExample);
            Guid professionalId = professionalResult.ProfessionalId;

            // Paciente inexistente
            Guid nonExistingPatientId = Guid.NewGuid();

            var request = RegisterHomeCareRequestGenerator.Generate(
                patientId: nonExistingPatientId,
                professionalId: professionalId,
                healthUnitId: healthUnitId
            );

            var useCase = new RegisterHomeCareUseCase(
                homeCareRepo,
                patientRepo,
                professionalRepo,
                healthUnitRepo);

            // Act
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Paciente informado não foi encontrado.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Profissional_Nao_For_Encontrado()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IHomeCareRepository homeCareRepo = new HomeCareRepository(context);
            var patientRepo = new PatientRepository(context);
            var professionalRepo = new ProfessionalRepository(context);
            var healthUnitRepo = new HealthUnitRepository(context);

            // ===== Cria HealthUnit válida =====
            var healthUnitRequest = HealthUnitGenerator.GenerateHealthUnit();
            var registerHealthUnitUseCase = new RegisterHealthUnitUseCase(healthUnitRepo);
            var healthUnitResult = await registerHealthUnitUseCase.Handle(healthUnitRequest);
            Guid healthUnitId = healthUnitResult.HealthUnitId;

            // ===== Cria Patient válido =====
            RegisterPatientRequest patientExample =
                PatientGenerator.GeneratePatient(isUnderage: false);

            ConsentDto treatmentConsentPatient =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsentPatient =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsentPatient =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);

            patientExample.Consents.Add(treatmentConsentPatient);
            patientExample.Consents.Add(researchConsentPatient);
            patientExample.Consents.Add(notificationConsentPatient);

            var registerPatientUseCase = new RegisterPatientUseCase(patientRepo);
            var patientResult = await registerPatientUseCase.Handle(patientExample);
            Guid patientId = patientResult.PatientId;

            // Profissional inexistente
            Guid nonExistingProfessionalId = Guid.NewGuid();

            var request = RegisterHomeCareRequestGenerator.Generate(
                patientId: patientId,
                professionalId: nonExistingProfessionalId,
                healthUnitId: healthUnitId
            );

            var useCase = new RegisterHomeCareUseCase(
                homeCareRepo,
                patientRepo,
                professionalRepo,
                healthUnitRepo);

            // Act
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Profissional informado não foi encontrado.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Unidade_De_Saude_Nao_For_Encontrada()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IHomeCareRepository homeCareRepo = new HomeCareRepository(context);
            var patientRepo = new PatientRepository(context);
            var professionalRepo = new ProfessionalRepository(context);
            var healthUnitRepo = new HealthUnitRepository(context);

            // ===== Cria Professional válido =====
            RegisterProfessionalRequest professionalExample =
                ProfessionalGenerator.GenerateProfessional(durationInMinutes: 30, weeklyWindowsCount: 0);

            ConsentDto treatmentConsentProf =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsentProf =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsentProf =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);

            professionalExample.Consents.Add(treatmentConsentProf);
            professionalExample.Consents.Add(researchConsentProf);
            professionalExample.Consents.Add(notificationConsentProf);

            var registerProfessionalUseCase = new RegisterProfessionalUseCase(professionalRepo);
            var professionalResult = await registerProfessionalUseCase.Handle(professionalExample);
            Guid professionalId = professionalResult.ProfessionalId;

            // ===== Cria Patient válido =====
            RegisterPatientRequest patientExample =
                PatientGenerator.GeneratePatient(isUnderage: false);

            ConsentDto treatmentConsentPatient =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsentPatient =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsentPatient =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);

            patientExample.Consents.Add(treatmentConsentPatient);
            patientExample.Consents.Add(researchConsentPatient);
            patientExample.Consents.Add(notificationConsentPatient);

            var registerPatientUseCase = new RegisterPatientUseCase(patientRepo);
            var patientResult = await registerPatientUseCase.Handle(patientExample);
            Guid patientId = patientResult.PatientId;

            // Unidade de saúde inexistente
            Guid nonExistingHealthUnitId = Guid.NewGuid();

            var request = RegisterHomeCareRequestGenerator.Generate(
                patientId: patientId,
                professionalId: professionalId,
                healthUnitId: nonExistingHealthUnitId
            );

            var useCase = new RegisterHomeCareUseCase(
                homeCareRepo,
                patientRepo,
                professionalRepo,
                healthUnitRepo);

            // Act
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Unidade de saúde informada não foi encontrada.");
        }
    }
}
