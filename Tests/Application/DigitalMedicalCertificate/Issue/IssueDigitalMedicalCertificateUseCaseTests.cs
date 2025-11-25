// Tests/Application/DigitalMedicalCertificate/Issue/IssueDigitalMedicalCertificateUseCaseTests.cs

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.DigitalMedicalCertificates.Issue;
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

namespace SGHSS.Tests.Application.DigitalMedicalCertificates.Issue
{
    public class IssueDigitalMedicalCertificateUseCaseTests
    {
        [Fact]
        public async Task Deve_Emitir_Atestado_Quando_Dados_Forem_Validos()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IPatientRepository patientRepo = new PatientRepository(context);
            IProfessionalRepository professionalRepo = new ProfessionalRepository(context);
            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);
            IDigitalMedicalCertificateRepository certRepo = new DigitalMedicalCertificateRepository(context);

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

            // Consulta associada ao atestado
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                StartTime = DateTimeOffset.UtcNow.AddHours(-1),
                EndTime = DateTimeOffset.UtcNow,
                Status = AppointmentStatus.Completed,
                Type = AppointmentType.Presential,
                Description = "Consulta clínica geral.",
                Patient = patient
            };

            // Navegações opcionais para coerência do modelo
            patient.Appointments.Add(appointment);
            healthUnit.Appointments.Add(appointment);

            await context.Appointments.AddAsync(appointment);
            await context.SaveChangesAsync();

            var issueUseCase = new IssueDigitalMedicalCertificateUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo,
                certRepo
            );

            var request = DigitalMedicalCertificateRequestGenerator.Generate(
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
            var certificate = await context.DigitalMedicalCertificates
                .Include(c => c.Patient)
                .Include(c => c.Professional)
                .Include(c => c.HealthUnit)
                .Include(c => c.Appointment)
                .FirstAsync(c => c.Id == response.CertificateId);

            certificate.Patient.Id.Should().Be(patientResult.PatientId);
            certificate.Professional.Id.Should().Be(professionalResult.ProfessionalId);
            certificate.HealthUnit.Id.Should().Be(healthUnitResult.HealthUnitId);
            certificate.Appointment.Id.Should().Be(appointment.Id);

            certificate.Recommendations.Should().Be(request.Recommendations);
            certificate.ValidUntil.Should().Be(request.ValidUntil);

            certificate.IssuedAt.Should().BeOnOrAfter(before);
            certificate.IssuedAt.Should().BeOnOrBefore(after);

            // Navegações inversas
            patient.DigitalMedicalCertificates.Should().Contain(c => c.Id == certificate.Id);
            professional.DigitalMedicalCertificates.Should().Contain(c => c.Id == certificate.Id);
            healthUnit.DigitalMedicalCertificates.Should().Contain(c => c.Id == certificate.Id);
            appointment.DigitalMedicalCertificate.Should().Be(certificate);

            // Assert – response
            response.Should().NotBeNull();
            response.PatientId.Should().Be(patientResult.PatientId);
            response.ProfessionalId.Should().Be(professionalResult.ProfessionalId);
            response.HealthUnitId.Should().Be(healthUnitResult.HealthUnitId);
            response.AppointmentId.Should().Be(appointment.Id);
            response.Recommendations.Should().Be(request.Recommendations);
            response.ValidUntil.Should().Be(request.ValidUntil);

            response.IssuedAt.Should().BeOnOrAfter(before);
            response.IssuedAt.Should().BeOnOrBefore(after);
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
            IDigitalMedicalCertificateRepository certRepo = new DigitalMedicalCertificateRepository(context);

            var issueUseCase = new IssueDigitalMedicalCertificateUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo,
                certRepo
            );

            var request = DigitalMedicalCertificateRequestGenerator.Generate(
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
        public async Task Deve_Lancar_Excecao_Quando_Profissional_Nao_For_Encontrado()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IPatientRepository patientRepo = new PatientRepository(context);
            IProfessionalRepository professionalRepo = new ProfessionalRepository(context);
            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);
            IDigitalMedicalCertificateRepository certRepo = new DigitalMedicalCertificateRepository(context);

            var registerPatientUseCase = new RegisterPatientUseCase(patientRepo);

            // Paciente válido
            RegisterPatientRequest patientExample = PatientGenerator.GeneratePatient();

            ConsentDto treatmentConsentPatient =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsentPatient =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);

            patientExample.Consents.Add(treatmentConsentPatient);
            patientExample.Consents.Add(researchConsentPatient);

            var patientResult = await registerPatientUseCase.Handle(patientExample);

            var issueUseCase = new IssueDigitalMedicalCertificateUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo,
                certRepo
            );

            var request = DigitalMedicalCertificateRequestGenerator.Generate(
                providedPatientId: patientResult.PatientId,
                providedProfessionalId: Guid.NewGuid()
            );

            // Act
            Func<Task> act = () => issueUseCase.Handle(request);

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
            IDigitalMedicalCertificateRepository certRepo = new DigitalMedicalCertificateRepository(context);

            var registerPatientUseCase = new RegisterPatientUseCase(patientRepo);
            var registerProfessionalUseCase = new RegisterProfessionalUseCase(professionalRepo);

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

            var issueUseCase = new IssueDigitalMedicalCertificateUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo,
                certRepo
            );

            var request = DigitalMedicalCertificateRequestGenerator.Generate(
                providedPatientId: patientResult.PatientId,
                providedProfessionalId: professionalResult.ProfessionalId,
                providedHealthUnitId: Guid.NewGuid()
            );

            // Act
            Func<Task> act = () => issueUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Unidade de saúde não encontrada para o identificador informado.");
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
            IDigitalMedicalCertificateRepository certRepo = new DigitalMedicalCertificateRepository(context);

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

            var issueUseCase = new IssueDigitalMedicalCertificateUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo,
                certRepo
            );

            var request = DigitalMedicalCertificateRequestGenerator.Generate(
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
        public async Task Deve_Lancar_Excecao_Quando_Recomendacoes_Estiverem_Vazias()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IPatientRepository patientRepo = new PatientRepository(context);
            IProfessionalRepository professionalRepo = new ProfessionalRepository(context);
            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);
            IDigitalMedicalCertificateRepository certRepo = new DigitalMedicalCertificateRepository(context);

            var issueUseCase = new IssueDigitalMedicalCertificateUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo,
                certRepo
            );

            var request = new IssueDigitalMedicalCertificateRequest
            {
                PatientId = Guid.NewGuid(),
                ProfessionalId = Guid.NewGuid(),
                HealthUnitId = Guid.NewGuid(),
                AppointmentId = Guid.NewGuid(),
                ValidUntil = DateTimeOffset.UtcNow.AddDays(2),
                Recommendations = " ", // vazio/whitespace
                IcpSignatureRaw = "MAMBAf8="
            };

            // Act
            Func<Task> act = () => issueUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("As recomendações do atestado não podem ser vazias.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Assinatura_Icp_For_Vazia()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IPatientRepository patientRepo = new PatientRepository(context);
            IProfessionalRepository professionalRepo = new ProfessionalRepository(context);
            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);
            IDigitalMedicalCertificateRepository certRepo = new DigitalMedicalCertificateRepository(context);

            var issueUseCase = new IssueDigitalMedicalCertificateUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo,
                certRepo
            );

            var request = new IssueDigitalMedicalCertificateRequest
            {
                PatientId = Guid.NewGuid(),
                ProfessionalId = Guid.NewGuid(),
                HealthUnitId = Guid.NewGuid(),
                AppointmentId = Guid.NewGuid(),
                ValidUntil = DateTimeOffset.UtcNow.AddDays(2),
                Recommendations = "Recomendação válida.",
                IcpSignatureRaw = " " // vazia/whitespace
            };

            // Act
            Func<Task> act = () => issueUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("A assinatura ICP do atestado é obrigatória.");
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
            IDigitalMedicalCertificateRepository certRepo = new DigitalMedicalCertificateRepository(context);

            var issueUseCase = new IssueDigitalMedicalCertificateUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo,
                certRepo
            );

            var request = new IssueDigitalMedicalCertificateRequest
            {
                PatientId = Guid.NewGuid(),
                ProfessionalId = Guid.NewGuid(),
                HealthUnitId = Guid.NewGuid(),
                AppointmentId = Guid.NewGuid(),
                ValidUntil = DateTimeOffset.UtcNow, // não é estritamente maior
                Recommendations = "Recomendação válida.",
                IcpSignatureRaw = "MAMBAf8="
            };

            // Act
            Func<Task> act = () => issueUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("A data de validade do atestado deve ser posterior à data de emissão.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Consulta_Nao_Pertencer_Ao_Paciente()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IPatientRepository patientRepo = new PatientRepository(context);
            IProfessionalRepository professionalRepo = new ProfessionalRepository(context);
            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);
            IDigitalMedicalCertificateRepository certRepo = new DigitalMedicalCertificateRepository(context);

            var registerPatientUseCase = new RegisterPatientUseCase(patientRepo);
            var registerProfessionalUseCase = new RegisterProfessionalUseCase(professionalRepo);
            var registerHealthUnitUseCase = new RegisterHealthUnitUseCase(healthUnitRepo);

            // Paciente A
            RegisterPatientRequest patientAExample = PatientGenerator.GeneratePatient();

            ConsentDto treatmentConsentA =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsentA =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);

            patientAExample.Consents.Add(treatmentConsentA);
            patientAExample.Consents.Add(researchConsentA);

            var patientAResult = await registerPatientUseCase.Handle(patientAExample);

            // Paciente B
            RegisterPatientRequest patientBExample = PatientGenerator.GeneratePatient();

            ConsentDto treatmentConsentB =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsentB =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);

            patientBExample.Consents.Add(treatmentConsentB);
            patientBExample.Consents.Add(researchConsentB);

            var patientBResult = await registerPatientUseCase.Handle(patientBExample);

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

            // Carrega paciente B e cria consulta associada a ele
            var patientB = await context.Patients.FirstAsync(p => p.Id == patientBResult.PatientId);

            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                StartTime = DateTimeOffset.UtcNow.AddHours(-1),
                EndTime = DateTimeOffset.UtcNow,
                Status = AppointmentStatus.Completed,
                Type = AppointmentType.Presential,
                Description = "Consulta do paciente B.",
                Patient = patientB
            };

            patientB.Appointments.Add(appointment);
            await context.Appointments.AddAsync(appointment);
            await context.SaveChangesAsync();

            var issueUseCase = new IssueDigitalMedicalCertificateUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo,
                certRepo
            );

            // Request usando paciente A, mas consulta de B
            var request = DigitalMedicalCertificateRequestGenerator.Generate(
                providedPatientId: patientAResult.PatientId,
                providedProfessionalId: professionalResult.ProfessionalId,
                providedHealthUnitId: healthUnitResult.HealthUnitId,
                providedAppointmentId: appointment.Id
            );

            // Act
            Func<Task> act = () => issueUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("A consulta informada não pertence ao paciente especificado.");
        }
    }
}
