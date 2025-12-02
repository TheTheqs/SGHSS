// Tests/Application/Appointments/Update/CompleteAppointmentUseCaseTests.cs

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.Appointments.Update;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.DigitalMedicalCertificates.Issue;
using SGHSS.Application.UseCases.EletronicPrescriptions.Issue;
using SGHSS.Application.UseCases.Patients.Register;
using SGHSS.Application.UseCases.Patients.Update;
using SGHSS.Application.UseCases.Professionals.Register;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Appointments;
using SGHSS.Tests.TestData.Models;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Patients;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Professionals;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.MedicalRecords;
using Xunit;
using SGHSS.Application.UseCases.Notifications.Create;

namespace SGHSS.Tests.Application.Appointments.Update
{
    public class CompleteAppointmentUseCaseTests
    {
        /// <summary>
        /// Cenário feliz básico: consulta confirmada, sem emissão de documentos adicionais.
        /// Deve apenas marcar a consulta e o slot como concluídos.
        /// </summary>
        [Fact]
        public async Task Deve_Concluir_Consulta_Sem_Documentos_Extras()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IPatientRepository patientRepo = new PatientRepository(context);
            IProfessionalRepository professionalRepo = new ProfessionalRepository(context);
            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);
            IDigitalMedicalCertificateRepository certificateRepo = new DigitalMedicalCertificateRepository(context);
            IEletronicPrescriptionRepository prescriptionRepo = new EletronicPrescriptionRepository(context);
            INotificationRepository notificationRepo = new NotificationRepository(context);
            IUserRepository userRepo = new UserRepository(context);

            var createNotificationUseCase = new CreateNotificationUseCase(notificationRepo, userRepo);
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

            // Carrega entidades rastreadas
            var patient = await context.Patients.FirstAsync(p => p.Id == patientResult.PatientId);
            var professional = await context.Professionals.FirstAsync(p => p.Id == professionalResult.ProfessionalId);
            var healthUnit = await context.HealthUnits.FirstAsync(h => h.Id == healthUnitResult.HealthUnitId);

            // Slot de agenda
            var slot = new ScheduleSlot
            {
                StartDateTime = DateTime.UtcNow.AddMinutes(-30),
                EndDateTime = DateTime.UtcNow,
                Status = ScheduleSlotStatus.Reserved
            };

            // Consulta confirmada
            var appointment = new Appointment
            {
                StartTime = DateTimeOffset.UtcNow.AddMinutes(-30),
                EndTime = DateTimeOffset.UtcNow,
                Status = AppointmentStatus.Confirmed,
                Type = AppointmentType.Presential,
                Description = "Consulta clínica geral.",
                Patient = patient,
                ScheduleSlot = slot
            };

            patient.Appointments.Add(appointment);
            healthUnit.Appointments.Add(appointment);

            await context.Appointments.AddAsync(appointment);
            await context.SaveChangesAsync();

            var issueCertificateUseCase = new IssueDigitalMedicalCertificateUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo,
                certificateRepo
            );

            var issuePrescriptionUseCase = new IssueEletronicPrescriptionUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo,
                prescriptionRepo
            );

            var updateMedicalRecordUseCase = new UpdateMedicalRecordUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo
            );

            var completeUseCase = new CompleteAppointmentUseCase(
                appointmentRepo,
                issueCertificateUseCase,
                issuePrescriptionUseCase,
                updateMedicalRecordUseCase,
                createNotificationUseCase
            );

            var request = CompleteAppointmentRequestGenerator.Generate(
                providedAppointmentId: appointment.Id,
                includeCertificate: false,
                includePrescription: false,
                includeMedicalRecordUpdate: false
            );

            // Act
            var response = await completeUseCase.Handle(request);

            // Assert – entidade
            var updatedAppointment = await context.Appointments
                .Include(a => a.ScheduleSlot)
                .FirstAsync(a => a.Id == appointment.Id);

            updatedAppointment.Status.Should().Be(AppointmentStatus.Completed);
            updatedAppointment.ScheduleSlot.Status.Should().Be(ScheduleSlotStatus.Completed);

            updatedAppointment.DigitalMedicalCertificate.Should().BeNull();
            updatedAppointment.EletronicPrescription.Should().BeNull();
            updatedAppointment.MedicalRecordUpdate.Should().BeNull();

            // Assert – response
            response.Should().NotBeNull();
            response.AppointmentId.Should().Be(appointment.Id);
            response.Status.Should().Be(AppointmentStatus.Completed);
            response.ScheduleSlotId.Should().Be(slot.Id);
            response.DigitalMedicalCertificateId.Should().BeNull();
            response.EletronicPrescriptionId.Should().BeNull();
            response.MedicalRecordUpdateId.Should().BeNull();
        }

        /// <summary>
        /// Cenário feliz completo: consulta confirmada com emissão de
        /// atestado, prescrição e atualização de prontuário.
        /// </summary>
        [Fact]
        public async Task Deve_Concluir_Consulta_Com_Atestado_Prescricao_E_Prontuario()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IPatientRepository patientRepo = new PatientRepository(context);
            IProfessionalRepository professionalRepo = new ProfessionalRepository(context);
            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);
            IDigitalMedicalCertificateRepository certificateRepo = new DigitalMedicalCertificateRepository(context);
            IEletronicPrescriptionRepository prescriptionRepo = new EletronicPrescriptionRepository(context);
            INotificationRepository notificationRepo = new NotificationRepository(context);
            IUserRepository userRepo = new UserRepository(context);

            var createNotificationUseCase = new CreateNotificationUseCase(notificationRepo, userRepo);
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

            // Carrega entidades rastreadas
            var patient = await context.Patients.FirstAsync(p => p.Id == patientResult.PatientId);
            var professional = await context.Professionals.FirstAsync(p => p.Id == professionalResult.ProfessionalId);
            var healthUnit = await context.HealthUnits.FirstAsync(h => h.Id == healthUnitResult.HealthUnitId);

            // Slot de agenda
            var slot = new ScheduleSlot
            {
                StartDateTime = DateTime.UtcNow.AddMinutes(-30),
                EndDateTime = DateTime.UtcNow,
                Status = ScheduleSlotStatus.Reserved
            };

            // Consulta confirmada
            var appointment = new Appointment
            {
                StartTime = DateTimeOffset.UtcNow.AddMinutes(-30),
                EndTime = DateTimeOffset.UtcNow,
                Status = AppointmentStatus.Confirmed,
                Type = AppointmentType.Presential,
                Description = "Consulta para emissão de documentos.",
                Patient = patient,
                ScheduleSlot = slot
            };

            patient.Appointments.Add(appointment);
            healthUnit.Appointments.Add(appointment);

            await context.Appointments.AddAsync(appointment);
            await context.SaveChangesAsync();

            var issueCertificateUseCase = new IssueDigitalMedicalCertificateUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo,
                certificateRepo
            );

            var issuePrescriptionUseCase = new IssueEletronicPrescriptionUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo,
                prescriptionRepo
            );

            var updateMedicalRecordUseCase = new UpdateMedicalRecordUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo
            );

            var completeUseCase = new CompleteAppointmentUseCase(
                appointmentRepo,
                issueCertificateUseCase,
                issuePrescriptionUseCase,
                updateMedicalRecordUseCase,
                createNotificationUseCase
            );

            // Requests filhos usando generators + overrides para IDs corretos
            var certificateRequest = DigitalMedicalCertificateRequestGenerator.Generate(
                providedPatientId: patientResult.PatientId,
                providedProfessionalId: professionalResult.ProfessionalId,
                providedHealthUnitId: healthUnitResult.HealthUnitId,
                providedAppointmentId: appointment.Id
            );

            var prescriptionRequest = EletronicPrescriptionRequestGenerator.Generate(
                providedPatientId: patientResult.PatientId,
                providedProfessionalId: professionalResult.ProfessionalId,
                providedHealthUnitId: healthUnitResult.HealthUnitId,
                providedAppointmentId: appointment.Id
            );

            var medicalRecordRequest = UpdateMedicalRecordRequestGenerator.Generate(
                providedPatientId: patientResult.PatientId,
                providedProfessionalId: professionalResult.ProfessionalId,
                providedHealthUnitId: healthUnitResult.HealthUnitId
            );

            var request = CompleteAppointmentRequestGenerator.Generate(
                providedAppointmentId: appointment.Id,
                includeCertificate: true,
                includePrescription: true,
                includeMedicalRecordUpdate: true,
                certificateOverride: certificateRequest,
                prescriptionOverride: prescriptionRequest,
                medicalRecordOverride: medicalRecordRequest
            );

            // Act
            var response = await completeUseCase.Handle(request);

            // Assert – entidade
            var updatedAppointment = await context.Appointments
                .Include(a => a.ScheduleSlot)
                .Include(a => a.DigitalMedicalCertificate)
                .Include(a => a.EletronicPrescription)
                .Include(a => a.MedicalRecordUpdate)
                .FirstAsync(a => a.Id == appointment.Id);

            updatedAppointment.Status.Should().Be(AppointmentStatus.Completed);
            updatedAppointment.ScheduleSlot.Status.Should().Be(ScheduleSlotStatus.Completed);

            updatedAppointment.DigitalMedicalCertificate.Should().NotBeNull();
            updatedAppointment.EletronicPrescription.Should().NotBeNull();
            updatedAppointment.MedicalRecordUpdate.Should().NotBeNull();

            // Assert – response coerente com as entidades criadas
            response.Should().NotBeNull();
            response.AppointmentId.Should().Be(appointment.Id);
            response.Status.Should().Be(AppointmentStatus.Completed);
            response.ScheduleSlotId.Should().Be(slot.Id);

            response.DigitalMedicalCertificateId.Should().Be(updatedAppointment.DigitalMedicalCertificate!.Id);
            response.EletronicPrescriptionId.Should().Be(updatedAppointment.EletronicPrescription!.Id);
            response.MedicalRecordUpdateId.Should().Be(updatedAppointment.MedicalRecordUpdate!.Id);
        }

        /// <summary>
        /// Deve lançar exceção quando a consulta não for encontrada.
        /// </summary>
        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Consulta_Nao_For_Encontrada()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IPatientRepository patientRepo = new PatientRepository(context);
            IProfessionalRepository professionalRepo = new ProfessionalRepository(context);
            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);
            IDigitalMedicalCertificateRepository certificateRepo = new DigitalMedicalCertificateRepository(context);
            IEletronicPrescriptionRepository prescriptionRepo = new EletronicPrescriptionRepository(context);
            INotificationRepository notificationRepo = new NotificationRepository(context);
            IUserRepository userRepo = new UserRepository(context);

            var createNotificationUseCase = new CreateNotificationUseCase(notificationRepo, userRepo);

            var issueCertificateUseCase = new IssueDigitalMedicalCertificateUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo,
                certificateRepo
            );

            var issuePrescriptionUseCase = new IssueEletronicPrescriptionUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo,
                prescriptionRepo
            );

            var updateMedicalRecordUseCase = new UpdateMedicalRecordUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo
            );

            var completeUseCase = new CompleteAppointmentUseCase(
                appointmentRepo,
                issueCertificateUseCase,
                issuePrescriptionUseCase,
                updateMedicalRecordUseCase,
                createNotificationUseCase
            );

            var request = CompleteAppointmentRequestGenerator.Generate(
                providedAppointmentId: Guid.NewGuid()
            );

            // Act
            Func<Task> act = () => completeUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Consulta informada não foi encontrada.");
        }

        /// <summary>
        /// Deve lançar exceção quando a consulta não estiver no status Confirmed.
        /// </summary>
        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Consulta_Nao_Estiver_Confirmada()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IPatientRepository patientRepo = new PatientRepository(context);
            IProfessionalRepository professionalRepo = new ProfessionalRepository(context);
            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);
            IDigitalMedicalCertificateRepository certificateRepo = new DigitalMedicalCertificateRepository(context);
            IEletronicPrescriptionRepository prescriptionRepo = new EletronicPrescriptionRepository(context);
            INotificationRepository notificationRepo = new NotificationRepository(context);
            IUserRepository userRepo = new UserRepository(context);

            var createNotificationUseCase = new CreateNotificationUseCase(notificationRepo, userRepo);
            var registerPatientUseCase = new RegisterPatientUseCase(patientRepo);

            // Paciente (mínimo necessário para criar consulta)
            RegisterPatientRequest patientExample = PatientGenerator.GeneratePatient();

            ConsentDto treatmentConsentPatient =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsentPatient =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);

            patientExample.Consents.Add(treatmentConsentPatient);
            patientExample.Consents.Add(researchConsentPatient);

            var patientResult = await registerPatientUseCase.Handle(patientExample);

            var patient = await context.Patients.FirstAsync(p => p.Id == patientResult.PatientId);

            var slot = new ScheduleSlot
            {
                StartDateTime = DateTime.UtcNow.AddMinutes(-30),
                EndDateTime = DateTime.UtcNow,
                Status = ScheduleSlotStatus.Reserved
            };

            // Consulta já concluída (ou outro status diferente de Confirmed)
            var appointment = new Appointment
            { 
                StartTime = DateTimeOffset.UtcNow.AddMinutes(-30),
                EndTime = DateTimeOffset.UtcNow,
                Status = AppointmentStatus.Completed,
                Type = AppointmentType.Presential,
                Description = "Consulta que não está mais confirmada.",
                Patient = patient,
                ScheduleSlot = slot
            };

            await context.Appointments.AddAsync(appointment);
            await context.SaveChangesAsync();

            var issueCertificateUseCase = new IssueDigitalMedicalCertificateUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo,
                certificateRepo
            );

            var issuePrescriptionUseCase = new IssueEletronicPrescriptionUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo,
                prescriptionRepo
            );

            var updateMedicalRecordUseCase = new UpdateMedicalRecordUseCase(
                patientRepo,
                professionalRepo,
                healthUnitRepo,
                appointmentRepo
            );

            var completeUseCase = new CompleteAppointmentUseCase(
                appointmentRepo,
                issueCertificateUseCase,
                issuePrescriptionUseCase,
                updateMedicalRecordUseCase,
                createNotificationUseCase
            );

            var request = CompleteAppointmentRequestGenerator.Generate(
                providedAppointmentId: appointment.Id
            );

            // Act
            Func<Task> act = () => completeUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Apenas consultas confirmadas podem ser concluídas.");
        }
    }
}
