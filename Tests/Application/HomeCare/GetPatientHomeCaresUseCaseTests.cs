// Tests/Application/HomeCares/GetPatientHomeCaresUseCaseTests.cs

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.HomeCares.Read;
using SGHSS.Application.UseCases.HomeCares.Register;
using SGHSS.Application.UseCases.Patients.Register;
using SGHSS.Application.UseCases.Professionals.Register;
using SGHSS.Domain.Enums;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Models;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Patients;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Professionals;
using Xunit;

namespace SGHSS.Tests.Application.HomeCares.Read
{
    public class GetPatientHomeCaresUseCaseTests
    {
        [Fact]
        public async Task Deve_Retornar_HomeCares_Quando_Paciente_Possuir_Registros()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IHomeCareRepository homeCareRepo = new HomeCareRepository(context);
            var patientRepo = new PatientRepository(context);
            var professionalRepo = new ProfessionalRepository(context);
            var healthUnitRepo = new HealthUnitRepository(context);

            // ===== Cria unidade de saúde válida =====
            var healthUnitRequest = HealthUnitGenerator.GenerateHealthUnit();
            var registerHealthUnitUseCase = new RegisterHealthUnitUseCase(healthUnitRepo);
            var healthUnitResult = await registerHealthUnitUseCase.Handle(healthUnitRequest);

            healthUnitResult.Should().NotBeNull();
            healthUnitResult.HealthUnitId.Should().NotBe(Guid.Empty);

            Guid healthUnitId = healthUnitResult.HealthUnitId;

            // ===== Cria profissional válido =====
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

            // ===== Cria paciente válido =====
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

            // ===== Registra dois atendimentos de HomeCare para o mesmo paciente =====
            var registerHomeCareUseCase = new RegisterHomeCareUseCase(
                homeCareRepo,
                patientRepo,
                professionalRepo,
                healthUnitRepo);

            var request1 = RegisterHomeCareRequestGenerator.Generate(
                patientId: patientId,
                professionalId: professionalId,
                healthUnitId: healthUnitId
            );

            var request2 = RegisterHomeCareRequestGenerator.Generate(
                patientId: patientId,
                professionalId: professionalId,
                healthUnitId: healthUnitId
            );

            var response1 = await registerHomeCareUseCase.Handle(request1);
            var response2 = await registerHomeCareUseCase.Handle(request2);

            response1.HomeCareId.Should().NotBe(Guid.Empty);
            response2.HomeCareId.Should().NotBe(Guid.Empty);

            var totalPersisted = await context.HomeCares.CountAsync();
            totalPersisted.Should().Be(2);

            var useCase = new GetPatientHomeCaresUseCase(
                homeCareRepo,
                patientRepo);

            var getRequest = new GetPatientHomeCaresRequest
            {
                PatientId = patientId
            };

            // Act
            var getResponse = await useCase.Handle(getRequest);

            // Assert
            getResponse.Should().NotBeNull();
            getResponse.HomeCares.Should().NotBeNullOrEmpty();
            getResponse.HomeCares.Count.Should().Be(2);

            // Todos devem ser da mesma unidade e ter profissional consistente
            getResponse.HomeCares
                .All(hc => hc.ProfessionalId == professionalId)
                .Should().BeTrue();

            getResponse.HomeCares
                .All(hc => hc.HealthUnitId == healthUnitId)
                .Should().BeTrue();

            // Datas coerentes com o que foi gerado
            getResponse.HomeCares
                .Select(hc => hc.Date)
                .Should()
                .OnlyContain(d => d != default);
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Paciente_Nao_For_Encontrado()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IHomeCareRepository homeCareRepo = new HomeCareRepository(context);
            var patientRepo = new PatientRepository(context);

            var useCase = new GetPatientHomeCaresUseCase(
                homeCareRepo,
                patientRepo);

            var getRequest = new GetPatientHomeCaresRequest
            {
                PatientId = Guid.NewGuid()
            };

            // Act
            Func<Task> act = () => useCase.Handle(getRequest);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Paciente informado não foi encontrado.");
        }
    }
}
