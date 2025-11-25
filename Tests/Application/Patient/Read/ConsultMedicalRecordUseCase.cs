// Tests/Application/Patients/Read/ConsultMedicalRecordUseCaseTests.cs

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.Patients.ConsultMedicalRecord;
using SGHSS.Application.UseCases.Patients.Register;
using SGHSS.Application.UseCases.Patients.Update;
using SGHSS.Application.UseCases.Professionals.Register;
using SGHSS.Domain.Enums;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Models;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Patients;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Professionals;
using Xunit;

namespace SGHSS.Tests.Application.Patients.Consult
{
    public class ConsultMedicalRecordUseCaseTests
    {
        [Fact]
        public async Task Deve_Consultar_Prontuario_Quando_Paciente_Possuir_Prontuario()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IPatientRepository patientRepo = new PatientRepository(context);
            IProfessionalRepository professionalRepo = new ProfessionalRepository(context);
            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);

            var registerPatientUseCase = new RegisterPatientUseCase(patientRepo);
            var registerProfessionalUseCase = new RegisterProfessionalUseCase(professionalRepo);
            var registerHealthUnitUseCase = new RegisterHealthUnitUseCase(healthUnitRepo);
            var updateMedicalRecordUseCase = new UpdateMedicalRecordUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo
            );

            // Paciente com consentimentos válidos
            RegisterPatientRequest patientExample = PatientGenerator.GeneratePatient();

            ConsentDto treatmentConsentPatient =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsentPatient =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);

            patientExample.Consents.Add(treatmentConsentPatient);
            patientExample.Consents.Add(researchConsentPatient);

            var patientResult = await registerPatientUseCase.Handle(patientExample);

            // Profissional com consentimentos válidos
            RegisterProfessionalRequest professionalExample =
                ProfessionalGenerator.GenerateProfessional(durationInMinutes: 30, weeklyWindowsCount: 0);

            ConsentDto treatmentConsentProfessional =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsentProfessional =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsentProfessional =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);

            professionalExample.Consents.Add(treatmentConsentProfessional);
            professionalExample.Consents.Add(researchConsentProfessional);
            professionalExample.Consents.Add(notificationConsentProfessional);

            var professionalResult = await registerProfessionalUseCase.Handle(professionalExample);

            // Unidade de saúde
            RegisterHealthUnitRequest healthUnitExample = HealthUnitGenerator.GenerateHealthUnit();
            var healthUnitResult = await registerHealthUnitUseCase.Handle(healthUnitExample);

            // Cria pelo menos uma atualização de prontuário para o paciente
            string firstDescription = "Paciente apresenta melhora clínica, manter conduta atual.";
            var firstUpdateRequest = new UpdateMedicalRecordRequest
            {
                PatientId = patientResult.PatientId,
                ProfessionalId = professionalResult.ProfessionalId,
                HealthUnitId = healthUnitResult.HealthUnitId,
                AppointmentId = null,
                Description = firstDescription
            };

            var firstUpdateResponse = await updateMedicalRecordUseCase.Handle(firstUpdateRequest);

            // Opcional: segunda atualização para validar ordenação por data (mais recente primeiro)
            string secondDescription = "Paciente está estável, considerar alta em 24 horas.";
            var secondUpdateRequest = new UpdateMedicalRecordRequest
            {
                PatientId = patientResult.PatientId,
                ProfessionalId = professionalResult.ProfessionalId,
                HealthUnitId = healthUnitResult.HealthUnitId,
                AppointmentId = null,
                Description = secondDescription
            };

            var secondUpdateResponse = await updateMedicalRecordUseCase.Handle(secondUpdateRequest);

            // UseCase sob teste
            var consultUseCase = new ConsultMedicalRecordUseCase(patientRepo);

            var consultRequest = new ConsultMedicalRecordRequest
            {
                PatientId = patientResult.PatientId
            };

            // Act
            var response = await consultUseCase.Handle(consultRequest);

            // Assert (banco)
            var reloadedPatient = await context.Patients
                .Include(p => p.MedicalRecord)
                    .ThenInclude(mr => mr.Updates)
                .FirstAsync(p => p.Id == patientResult.PatientId);

            reloadedPatient.MedicalRecord.Should().NotBeNull();
            var medicalRecord = reloadedPatient.MedicalRecord;

            medicalRecord.Updates.Should().HaveCount(2);

            // Assert (response básico)
            response.Should().NotBeNull();
            response.PatientId.Should().Be(patientResult.PatientId);

            response.MedicalRecord.Should().NotBeNull();
            response.MedicalRecord.Id.Should().Be(medicalRecord.Id);
            response.MedicalRecord.Number.Should().NotBeNullOrWhiteSpace();
            response.MedicalRecord.CreatedAt.Should().Be(medicalRecord.CreatedAt);

            response.MedicalRecord.Updates.Should().HaveCount(2);

            // Atualização mais recente deve ser a última registrada (ordem decrescente por data)
            var orderedDescriptions = response.MedicalRecord
                .Updates
                .Select(u => u.Description)
                .ToList();

            orderedDescriptions.Should().HaveCount(2);
            orderedDescriptions[0].Should().Be(secondDescription);
            orderedDescriptions[1].Should().Be(firstDescription);

            // Garantia básica: IDs retornados no DTO devem existir nas entidades de domínio
            var dtoUpdateIds = response.MedicalRecord.Updates.Select(u => u.Id).ToList();
            var domainUpdateIds = medicalRecord.Updates.Select(u => u.Id).ToList();

            dtoUpdateIds.Should().BeEquivalentTo(domainUpdateIds);
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Paciente_Nao_For_Encontrado()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IPatientRepository patientRepo = new PatientRepository(context);

            var consultUseCase = new ConsultMedicalRecordUseCase(patientRepo);

            var request = new ConsultMedicalRecordRequest
            {
                PatientId = Guid.NewGuid()
            };

            // Act
            Func<Task> act = () => consultUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Não foi possível localizar um paciente para o identificador informado.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Identificador_De_Paciente_For_Vazio()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IPatientRepository patientRepo = new PatientRepository(context);

            var consultUseCase = new ConsultMedicalRecordUseCase(patientRepo);

            var request = new ConsultMedicalRecordRequest
            {
                PatientId = Guid.Empty
            };

            // Act
            Func<Task> act = () => consultUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("O identificador do paciente não pode ser vazio.*");
        }
    }
}
