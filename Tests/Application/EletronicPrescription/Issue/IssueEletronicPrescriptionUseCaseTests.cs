// Tests/Application/EletronicPrescription/Issue/IssueEletronicPrescriptionUseCaseTests.cs


using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.EletronicPrescriptions.Issue;
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

namespace SGHSS.Tests.Application.EletronicPrescriptions.Issue
{
    public class IssueEletronicPrescriptionUseCaseTests
    {
        [Fact]
        public async Task Deve_Emitir_Prescricao_Quando_Dados_Forem_Validos()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IPatientRepository patientRepo = new PatientRepository(context);
            IProfessionalRepository professionalRepo = new ProfessionalRepository(context);
            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);
            IEletronicPrescriptionRepository prescriptionRepo = new EletronicPrescriptionRepository(context);

            var registerPatientUseCase = new RegisterPatientUseCase(patientRepo);
            var registerProfessionalUseCase = new RegisterProfessionalUseCase(professionalRepo);
            var registerHealthUnitUseCase = new RegisterHealthUnitUseCase(healthUnitRepo);

            // Paciente (com consentimento de tratamento para criação de prontuário)
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

            // Carrega entidades rastreadas para relacionar na consulta
            var patient = await context.Patients.FirstAsync(p => p.Id == patientResult.PatientId);
            var professional = await context.Professionals.FirstAsync(p => p.Id == professionalResult.ProfessionalId);
            var healthUnit = await context.HealthUnits.FirstAsync(h => h.Id == healthUnitResult.HealthUnitId);

            // Consulta associada à prescrição
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                StartTime = DateTimeOffset.UtcNow.AddHours(-1),
                EndTime = DateTimeOffset.UtcNow,
                Status = AppointmentStatus.Completed,
                Type = AppointmentType.Presential,
                Description = "Consulta clínica geral para prescrição.",
                Patient = patient
            };

            // Navegações opcionais para coerência do modelo
            patient.Appointments.Add(appointment);
            healthUnit.Appointments.Add(appointment);

            await context.Appointments.AddAsync(appointment);
            await context.SaveChangesAsync();

            var issueUseCase = new IssueEletronicPrescriptionUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo,
                prescriptionRepo
            );

            var request = EletronicPrescriptionRequestGenerator.Generate(
                providedPatientId: patientResult.PatientId,
                providedProfessionalId: professionalResult.ProfessionalId,
                providedHealthUnitId: healthUnitResult.HealthUnitId,
                providedAppointmentId: appointment.Id
            );

            var before = DateTimeOffset.UtcNow;

            // Act
            var response = await issueUseCase.Handle(request);

            var after = DateTimeOffset.UtcNow;

            // Assert – entidade
            var prescription = await context.EletronicPrescriptions
                .Include(c => c.Patient)
                .Include(c => c.Professional)
                .Include(c => c.HealthUnit)
                .Include(c => c.Appointment)
                .FirstAsync(c => c.Id == response.PrescriptionId);

            prescription.Patient.Id.Should().Be(patientResult.PatientId);
            prescription.Professional.Id.Should().Be(professionalResult.ProfessionalId);
            prescription.HealthUnit.Id.Should().Be(healthUnitResult.HealthUnitId);
            prescription.Appointment.Id.Should().Be(appointment.Id);

            prescription.Instructions.Should().Be(request.Instructions);
            prescription.ValidUntil.Should().Be(request.ValidUntil);

            prescription.CreatedAt.Should().BeOnOrAfter(before);
            prescription.CreatedAt.Should().BeOnOrBefore(after);

            // Navegações inversas
            patient.EletronicPrescriptions.Should().Contain(c => c.Id == prescription.Id);
            professional.EletronicPrescriptions.Should().Contain(c => c.Id == prescription.Id);
            healthUnit.EletronicPrescriptions.Should().Contain(c => c.Id == prescription.Id);
            appointment.EletronicPrescription.Should().Be(prescription);

            // Assert – response
            response.Should().NotBeNull();
            response.PatientId.Should().Be(patientResult.PatientId);
            response.ProfessionalId.Should().Be(professionalResult.ProfessionalId);
            response.HealthUnitId.Should().Be(healthUnitResult.HealthUnitId);
            response.AppointmentId.Should().Be(appointment.Id);
            response.Instructions.Should().Be(request.Instructions);
            response.ValidUntil.Should().Be(request.ValidUntil);

            response.CreatedAt.Should().BeOnOrAfter(before);
            response.CreatedAt.Should().BeOnOrBefore(after);
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
            IEletronicPrescriptionRepository prescriptionRepo = new EletronicPrescriptionRepository(context);

            var issueUseCase = new IssueEletronicPrescriptionUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo,
                prescriptionRepo
            );

            var request = EletronicPrescriptionRequestGenerator.Generate(
                providedPatientId: Guid.NewGuid()
            );

            // Act
            Func<Task> act = () => issueUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Paciente não encontrado para o identificador informado.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Consulta_Nao_For_Encontrada()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IPatientRepository patientRepo = new PatientRepository(context);
            IProfessionalRepository professionalRepo = new ProfessionalRepository(context);
            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);
            IEletronicPrescriptionRepository prescriptionRepo = new EletronicPrescriptionRepository(context);

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

            var issueUseCase = new IssueEletronicPrescriptionUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo,
                prescriptionRepo
            );

            var request = EletronicPrescriptionRequestGenerator.Generate(
                providedPatientId: patientResult.PatientId,
                providedProfessionalId: professionalResult.ProfessionalId,
                providedHealthUnitId: healthUnitResult.HealthUnitId,
                providedAppointmentId: Guid.NewGuid() // consulta inexistente
            );

            // Act
            Func<Task> act = () => issueUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Consulta não encontrada para o identificador informado.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Instrucoes_Estiverem_Vazias()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IPatientRepository patientRepo = new PatientRepository(context);
            IProfessionalRepository professionalRepo = new ProfessionalRepository(context);
            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);
            IEletronicPrescriptionRepository prescriptionRepo = new EletronicPrescriptionRepository(context);

            var issueUseCase = new IssueEletronicPrescriptionUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo,
                prescriptionRepo
            );

            var request = new IssueEletronicPrescriptionRequest
            {
                PatientId = Guid.NewGuid(),
                ProfessionalId = Guid.NewGuid(),
                HealthUnitId = Guid.NewGuid(),
                AppointmentId = Guid.NewGuid(),
                ValidUntil = DateTimeOffset.UtcNow.AddDays(2),
                Instructions = " ", // vazio/whitespace
                IcpSignatureRaw = "MAMBAf8="
            };

            // Act
            Func<Task> act = () => issueUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("As instruções da prescrição não podem ser vazias.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Data_De_Validade_For_Passada_Ou_Igual_A_Agora()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IPatientRepository patientRepo = new PatientRepository(context);
            IProfessionalRepository professionalRepo = new ProfessionalRepository(context);
            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);
            IEletronicPrescriptionRepository prescriptionRepo = new EletronicPrescriptionRepository(context);

            var issueUseCase = new IssueEletronicPrescriptionUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo,
                prescriptionRepo
            );

            var request = new IssueEletronicPrescriptionRequest
            {
                PatientId = Guid.NewGuid(),
                ProfessionalId = Guid.NewGuid(),
                HealthUnitId = Guid.NewGuid(),
                AppointmentId = Guid.NewGuid(),
                ValidUntil = DateTimeOffset.UtcNow, // não é estritamente maior
                Instructions = "Tomar 1 comprimido a cada 8 horas.",
                IcpSignatureRaw = "MAMBAf8="
            };

            // Act
            Func<Task> act = () => issueUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("A data de validade da prescrição deve ser posterior à data de criação.");
        }
    }
}
