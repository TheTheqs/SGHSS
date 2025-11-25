using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.Common;
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

namespace SGHSS.Tests.Application.Patients.Update
{
    public class UpdateMedicalRecordUseCaseTests
    {
        [Fact]
        public async Task Deve_Registrar_Atualizacao_De_Prontuario_Quando_Dados_Forem_Validos()
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

            // Paciente
            RegisterPatientRequest patientExample = PatientGenerator.GeneratePatient();

            ConsentDto treatmentConsentPatient =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsentPatient =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);

            patientExample.Consents.Add(treatmentConsentPatient);
            patientExample.Consents.Add(researchConsentPatient);

            var patientResult = await registerPatientUseCase.Handle(patientExample);

            // Profissional
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

            // UseCase sob teste
            var updateUseCase = new UpdateMedicalRecordUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo
            );

            string description = "Paciente apresenta melhora clínica, manter conduta atual.";

            var request = new UpdateMedicalRecordRequest
            {
                PatientId = patientResult.PatientId,
                ProfessionalId = professionalResult.ProfessionalId,
                HealthUnitId = healthUnitResult.HealthUnitId,
                AppointmentId = null, // cenário atual: sempre null
                Description = description
            };

            // Act
            var response = await updateUseCase.Handle(request);

            // Assert (banco)
            var reloadedPatient = await context.Patients
                .Include(p => p.MedicalRecord)
                    .ThenInclude(mr => mr.Updates)
                .FirstAsync(p => p.Id == patientResult.PatientId);

            reloadedPatient.MedicalRecord.Should().NotBeNull();
            var medicalRecord = reloadedPatient.MedicalRecord;

            medicalRecord.Updates.Should().HaveCount(1);
            var update = medicalRecord.Updates.Single();

            update.Description.Should().Be(description);
            update.MedicalRecord.Should().Be(medicalRecord);
            update.Professional.Id.Should().Be(professionalResult.ProfessionalId);
            update.HealthUnit.Id.Should().Be(healthUnitResult.HealthUnitId);
            update.Appointment.Should().BeNull();

            // Assert (response)
            response.Should().NotBeNull();
            response.MedicalRecordId.Should().Be(medicalRecord.Id);
            response.MedicalRecordUpdateId.Should().Be(update.Id);
            response.Description.Should().Be(description);

            // Garantia básica sobre a data (não precisa ser exata)
            response.UpdateDate.Should().BeCloseTo(update.UpdateDate, precision: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Paciente_Nao_For_Encontrado()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IPatientRepository patientRepo = new PatientRepository(context);
            IProfessionalRepository professionalRepo = new ProfessionalRepository(context);
            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);

            var updateUseCase = new UpdateMedicalRecordUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo
            );

            var request = new UpdateMedicalRecordRequest
            {
                PatientId = Guid.NewGuid(),
                ProfessionalId = Guid.NewGuid(),
                HealthUnitId = Guid.NewGuid(),
                AppointmentId = null,
                Description = "Qualquer descrição válida."
            };

            // Act
            Func<Task> act = () => updateUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Paciente não encontrado para o identificador informado.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Profissional_Nao_For_Encontrado()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IPatientRepository patientRepo = new PatientRepository(context);
            IProfessionalRepository professionalRepo = new ProfessionalRepository(context);
            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);

            var registerPatientUseCase = new RegisterPatientUseCase(patientRepo);
            var registerHealthUnitUseCase = new RegisterHealthUnitUseCase(healthUnitRepo);

            // Paciente com prontuário
            RegisterPatientRequest patientExample = PatientGenerator.GeneratePatient();

            ConsentDto treatmentConsentPatient =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsentPatient =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);

            patientExample.Consents.Add(treatmentConsentPatient);
            patientExample.Consents.Add(researchConsentPatient);

            var patientResult = await registerPatientUseCase.Handle(patientExample);

            // Unidade de saúde válida
            RegisterHealthUnitRequest healthUnitExample = HealthUnitGenerator.GenerateHealthUnit();
            var healthUnitResult = await registerHealthUnitUseCase.Handle(healthUnitExample);

            var updateUseCase = new UpdateMedicalRecordUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo
            );

            var request = new UpdateMedicalRecordRequest
            {
                PatientId = patientResult.PatientId,
                ProfessionalId = Guid.NewGuid(), // profissional inexistente
                HealthUnitId = healthUnitResult.HealthUnitId,
                AppointmentId = null,
                Description = "Descrição válida."
            };

            // Act
            Func<Task> act = () => updateUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Profissional não encontrado para o identificador informado.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Unidade_De_Saude_Nao_For_Encontrada()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IPatientRepository patientRepo = new PatientRepository(context);
            IProfessionalRepository professionalRepo = new ProfessionalRepository(context);
            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);

            var registerPatientUseCase = new RegisterPatientUseCase(patientRepo);
            var registerProfessionalUseCase = new RegisterProfessionalUseCase(professionalRepo);

            // Paciente com prontuário
            RegisterPatientRequest patientExample = PatientGenerator.GeneratePatient();

            ConsentDto treatmentConsentPatient =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsentPatient =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);

            patientExample.Consents.Add(treatmentConsentPatient);
            patientExample.Consents.Add(researchConsentPatient);

            var patientResult = await registerPatientUseCase.Handle(patientExample);

            // Profissional válido
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

            var updateUseCase = new UpdateMedicalRecordUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo
            );

            var request = new UpdateMedicalRecordRequest
            {
                PatientId = patientResult.PatientId,
                ProfessionalId = professionalResult.ProfessionalId,
                HealthUnitId = Guid.NewGuid(), // unidade inexistente
                AppointmentId = null,
                Description = "Descrição válida."
            };

            // Act
            Func<Task> act = () => updateUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Unidade de saúde não encontrada para o identificador informado.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Descricao_Estiver_Vazia()
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

            // Paciente
            RegisterPatientRequest patientExample = PatientGenerator.GeneratePatient();

            ConsentDto treatmentConsentPatient =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsentPatient =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);

            patientExample.Consents.Add(treatmentConsentPatient);
            patientExample.Consents.Add(researchConsentPatient);

            var patientResult = await registerPatientUseCase.Handle(patientExample);

            // Profissional
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

            var updateUseCase = new UpdateMedicalRecordUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo
            );

            var request = new UpdateMedicalRecordRequest
            {
                PatientId = patientResult.PatientId,
                ProfessionalId = professionalResult.ProfessionalId,
                HealthUnitId = healthUnitResult.HealthUnitId,
                AppointmentId = null,
                Description = " " // vazia/whitespace
            };

            // Act
            Func<Task> act = () => updateUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("A descrição da atualização de prontuário não pode ser vazia.");
        }
    }
}
