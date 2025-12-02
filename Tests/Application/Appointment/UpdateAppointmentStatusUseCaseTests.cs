// Tests/Application/Appointments/Update/UpdateAppointmentStatusUseCaseTests.cs

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Appointments.Update;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.Notifications.Create;
using SGHSS.Application.UseCases.Patients.Register;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Appointments;
using SGHSS.Tests.TestData.Models;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Patients;
using Xunit;

namespace SGHSS.Tests.Application.Appointments.Update
{
    public class UpdateAppointmentStatusUseCaseTests
    {
        /// <summary>
        /// Cenário feliz: consulta confirmada com slot reservado deve ter
        /// os status atualizados conforme o request.
        /// </summary>
        [Fact]
        public async Task Deve_Atualizar_Status_Quando_Consulta_For_Valida()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IPatientRepository patientRepo = new PatientRepository(context);
            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);
            INotificationRepository notificationRepo = new NotificationRepository(context);
            IUserRepository userRepo = new UserRepository(context);

            var createNotificationUseCase = new CreateNotificationUseCase(notificationRepo, userRepo);
            var registerPatientUseCase = new RegisterPatientUseCase(patientRepo);

            // Paciente com consentimentos mínimos
            RegisterPatientRequest patientExample = PatientGenerator.GeneratePatient();

            ConsentDto treatmentConsentPatient =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsentPatient =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);

            patientExample.Consents.Add(treatmentConsentPatient);
            patientExample.Consents.Add(researchConsentPatient);

            var patientResult = await registerPatientUseCase.Handle(patientExample);

            var patient = await context.Patients.FirstAsync(p => p.Id == patientResult.PatientId);

            // Slot de agenda
            var slot = new ScheduleSlot
            {
                Id = Guid.NewGuid(),
                StartDateTime = DateTime.UtcNow.AddMinutes(-30),
                EndDateTime = DateTime.UtcNow,
                Status = ScheduleSlotStatus.Reserved
            };

            // Consulta confirmada
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                StartTime = DateTimeOffset.UtcNow.AddMinutes(-30),
                EndTime = DateTimeOffset.UtcNow,
                Status = AppointmentStatus.Confirmed,
                Type = AppointmentType.Presential,
                Description = "Consulta para teste de atualização de status.",
                Patient = patient,
                ScheduleSlot = slot
            };

            patient.Appointments.Add(appointment);

            await context.Appointments.AddAsync(appointment);
            await context.SaveChangesAsync();

            var useCase = new UpdateAppointmentStatusUseCase(appointmentRepo, createNotificationUseCase);

            var request = UpdateAppointmentStatusRequestGenerator.Generate(
                providedAppointmentId: appointment.Id,
                providedAppointmentStatus: AppointmentStatus.Canceled,
                providedScheduleSlotStatus: ScheduleSlotStatus.Completed
            );

            // Act
            var response = await useCase.Handle(request);

            // Assert – entidade
            var updatedAppointment = await context.Appointments
                .Include(a => a.ScheduleSlot)
                .FirstAsync(a => a.Id == appointment.Id);

            updatedAppointment.Status.Should().Be(AppointmentStatus.Canceled);
            updatedAppointment.ScheduleSlot.Status.Should().Be(ScheduleSlotStatus.Completed);

            // Assert – response
            response.Should().NotBeNull();
            response.AppointmentId.Should().Be(appointment.Id);
            response.ScheduleSlotId.Should().Be(slot.Id);
            response.AppointmentStatus.Should().Be(AppointmentStatus.Canceled);
            response.ScheduleSlotStatus.Should().Be(ScheduleSlotStatus.Completed);
        }

        /// <summary>
        /// Deve lançar exceção quando a consulta não for encontrada.
        /// </summary>
        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Consulta_Nao_For_Encontrada()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);
            INotificationRepository notificationRepo = new NotificationRepository(context);
            IUserRepository userRepo = new UserRepository(context);

            var createNotificationUseCase = new CreateNotificationUseCase(notificationRepo, userRepo);
            var useCase = new UpdateAppointmentStatusUseCase(appointmentRepo, createNotificationUseCase);

            var request = UpdateAppointmentStatusRequestGenerator.Generate(
                providedAppointmentId: Guid.NewGuid()
            );

            // Act
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Consulta informada não foi encontrada.");
        }

        /// <summary>
        /// Deve lançar exceção quando a consulta já estiver concluída.
        /// </summary>
        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Consulta_Ja_Estiver_Concluida()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IPatientRepository patientRepo = new PatientRepository(context);
            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);
            INotificationRepository notificationRepo = new NotificationRepository(context);
            IUserRepository userRepo = new UserRepository(context);

            var createNotificationUseCase = new CreateNotificationUseCase(notificationRepo, userRepo);
            var registerPatientUseCase = new RegisterPatientUseCase(patientRepo);

            // Paciente
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
                Id = Guid.NewGuid(),
                StartDateTime = DateTime.UtcNow.AddMinutes(-30),
                EndDateTime = DateTime.UtcNow,
                Status = ScheduleSlotStatus.Completed
            };

            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                StartTime = DateTimeOffset.UtcNow.AddMinutes(-30),
                EndTime = DateTimeOffset.UtcNow,
                Status = AppointmentStatus.Completed,
                Type = AppointmentType.Presential,
                Description = "Consulta já concluída.",
                Patient = patient,
                ScheduleSlot = slot
            };

            patient.Appointments.Add(appointment);

            await context.Appointments.AddAsync(appointment);
            await context.SaveChangesAsync();

            var useCase = new UpdateAppointmentStatusUseCase(appointmentRepo, createNotificationUseCase);

            var request = UpdateAppointmentStatusRequestGenerator.Generate(
                providedAppointmentId: appointment.Id,
                providedAppointmentStatus: AppointmentStatus.Canceled,
                providedScheduleSlotStatus: ScheduleSlotStatus.Reserved
            );

            // Act
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Consultas concluídas ou canceladas não podem ter o status alterado.");
        }

        /// <summary>
        /// Deve lançar exceção quando a consulta já estiver cancelada.
        /// </summary>
        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Consulta_Ja_Estiver_Cancelada()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IPatientRepository patientRepo = new PatientRepository(context);
            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);
            INotificationRepository notificationRepo = new NotificationRepository(context);
            IUserRepository userRepo = new UserRepository(context);

            var createNotificationUseCase = new CreateNotificationUseCase(notificationRepo, userRepo);
            var registerPatientUseCase = new RegisterPatientUseCase(patientRepo);

            // Paciente
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
                Id = Guid.NewGuid(),
                StartDateTime = DateTime.UtcNow.AddMinutes(-30),
                EndDateTime = DateTime.UtcNow,
                Status = ScheduleSlotStatus.Completed
            };

            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                StartTime = DateTimeOffset.UtcNow.AddMinutes(-30),
                EndTime = DateTimeOffset.UtcNow,
                Status = AppointmentStatus.Canceled,
                Type = AppointmentType.Presential,
                Description = "Consulta já cancelada.",
                Patient = patient,
                ScheduleSlot = slot
            };

            patient.Appointments.Add(appointment);

            await context.Appointments.AddAsync(appointment);
            await context.SaveChangesAsync();

            var useCase = new UpdateAppointmentStatusUseCase(appointmentRepo, createNotificationUseCase);

            var request = UpdateAppointmentStatusRequestGenerator.Generate(
                providedAppointmentId: appointment.Id,
                providedAppointmentStatus: AppointmentStatus.Confirmed,
                providedScheduleSlotStatus: ScheduleSlotStatus.Reserved
            );

            // Act
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Consultas concluídas ou canceladas não podem ter o status alterado.");
        }
    }
}
