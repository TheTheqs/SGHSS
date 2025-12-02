// Tests/Application/Appointment/ScheduleAppointmentUseCaseTests.cs

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Appointments.Register;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.Notifications.Create;
using SGHSS.Application.UseCases.Patients.Register;
using SGHSS.Application.UseCases.Professionals.Register;
using SGHSS.Application.UseCases.ProfessionalSchedules.Consult;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Appointments;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Patients;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Professionals;
using Xunit;

namespace SGHSS.Tests.Application.Appointments.Register
{
    public class ScheduleAppointmentUseCaseTests
    {
        [Fact]
        public async Task Deve_Agendar_Consulta_Quando_Slot_Valido()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            var professionalRepo = new ProfessionalRepository(context);
            var patientRepo = new PatientRepository(context);
            IProfessionalScheduleRepository scheduleRepo = new ProfessionalScheduleRepository(context);
            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);
            INotificationRepository notificationRepo = new NotificationRepository(context);
            IUserRepository userRepo = new UserRepository(context);

            var createNotificationUseCase = new CreateNotificationUseCase(notificationRepo, userRepo);

            // Profissional
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
            var professionalId = professionalResult.ProfessionalId;

            // Ajusta política para algo determinístico
            var schedule = await context.ProfessionalSchedules
                .Include(ps => ps.SchedulePolicy)
                    .ThenInclude(sp => sp.WeeklyWindows)
                .Include(ps => ps.Professional)
                .FirstAsync(ps => ps.Professional.Id == professionalId);

            schedule.SchedulePolicy.DurationInMinutes = 30;
            schedule.SchedulePolicy.WeeklyWindows.Clear();

            schedule.SchedulePolicy.WeeklyWindows.Add(new WeeklyWindow
            {
                DayOfWeek = WeekDay.Monday,
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(10, 0),
                SchedulePolicy = schedule.SchedulePolicy
            });

            // Paciente
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
            var patientId = patientResult.PatientId;

            // Gera slots disponíveis
            var generateSlotsUseCase = new GenerateAvailableSlotsUseCase(scheduleRepo);

            DateTime baseDate = new DateTime(2025, 1, 6, 8, 0, 0); // Segunda-feira
            DateTime from = baseDate;
            DateTime to = baseDate.AddHours(2); // 08:00–10:00

            var slotsRequest = new GenerateAvailableSlotsRequest
            {
                ProfessionalId = professionalId,
                From = from,
                To = to
            };

            var slotsResponse = await generateSlotsUseCase.Handle(slotsRequest);
            slotsResponse.Slots.Should().NotBeNullOrEmpty();

            ScheduleSlotDto chosenSlot = slotsResponse.Slots.First();

            // Monta request de agendamento
            var appointmentRequest = AppointmentGenerator.GenerateAppointment(
                professionalId: professionalId,
                patientId: patientId,
                slot: chosenSlot,
                type: null,
                description: "Consulta de rotina."
            );

            var scheduleAppointmentUseCase = new ScheduleAppointmentUseCase(
                scheduleRepo,
                patientRepo,
                appointmentRepo,
                createNotificationUseCase);

            // Act
            var response = await scheduleAppointmentUseCase.Handle(appointmentRequest);

            // Assert
            var savedAppointment = await context.Appointments
                .Include(a => a.ScheduleSlot)
                    .ThenInclude(ss => ss.ProfessionalSchedule)
                        .ThenInclude(ps => ps.Professional)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync();

            savedAppointment.Should().NotBeNull();
            savedAppointment!.Patient.Should().NotBeNull();
            savedAppointment.Patient.Id.Should().Be(patientId);
            savedAppointment.ScheduleSlot.Should().NotBeNull();
            savedAppointment.ScheduleSlot.ProfessionalSchedule.Professional.Id.Should().Be(professionalId);

            savedAppointment.Status.Should().Be(AppointmentStatus.Confirmed);
            savedAppointment.ScheduleSlot.Status.Should().Be(ScheduleSlotStatus.Reserved);

            savedAppointment.StartTime.UtcDateTime.Should().Be(chosenSlot.StartDateTime);
            savedAppointment.EndTime.UtcDateTime.Should().Be(chosenSlot.EndDateTime);

            savedAppointment.Link.Should().NotBeNull();
        }

        [Fact]
        public async Task Nao_Deve_Permitir_Agendar_Slot_Ja_Ocupado()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            var professionalRepo = new ProfessionalRepository(context);
            var patientRepo = new PatientRepository(context);
            IProfessionalScheduleRepository scheduleRepo = new ProfessionalScheduleRepository(context);
            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);
            INotificationRepository notificationRepo = new NotificationRepository(context);
            IUserRepository userRepo = new UserRepository(context);
            var createNotificationUseCase = new CreateNotificationUseCase(notificationRepo, userRepo);

            // Profissional
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
            var professionalId = professionalResult.ProfessionalId;

            // Ajusta política determinística
            var schedule = await context.ProfessionalSchedules
                .Include(ps => ps.SchedulePolicy)
                    .ThenInclude(sp => sp.WeeklyWindows)
                .Include(ps => ps.Professional)
                .FirstAsync(ps => ps.Professional.Id == professionalId);

            schedule.SchedulePolicy.DurationInMinutes = 30;
            schedule.SchedulePolicy.WeeklyWindows.Clear();

            schedule.SchedulePolicy.WeeklyWindows.Add(new WeeklyWindow
            {
                DayOfWeek = WeekDay.Monday,
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(10, 0),
                SchedulePolicy = schedule.SchedulePolicy
            });

            // Paciente
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
            var patientId = patientResult.PatientId;

            // Slots disponíveis
            var generateSlotsUseCase = new GenerateAvailableSlotsUseCase(scheduleRepo);

            DateTime baseDate = new DateTime(2025, 1, 6, 8, 0, 0);
            DateTime from = baseDate;
            DateTime to = baseDate.AddHours(2);

            var slotsRequest = new GenerateAvailableSlotsRequest
            {
                ProfessionalId = professionalId,
                From = from,
                To = to
            };

            var slotsResponse = await generateSlotsUseCase.Handle(slotsRequest);
            slotsResponse.Slots.Should().NotBeNullOrEmpty();

            ScheduleSlotDto chosenSlot = slotsResponse.Slots.First();

            var scheduleAppointmentUseCase = new ScheduleAppointmentUseCase(
                scheduleRepo,
                patientRepo,
                appointmentRepo,
                createNotificationUseCase);

            // Primeiro agendamento (deve funcionar)
            var firstRequest = AppointmentGenerator.GenerateAppointment(
                professionalId: professionalId,
                patientId: patientId,
                slot: chosenSlot,
                description: "Primeira consulta."
            );

            var firstResponse = await scheduleAppointmentUseCase.Handle(firstRequest);
            firstResponse.Should().NotBeNull();

            // Segundo agendamento no MESMO slot
            var secondRequest = AppointmentGenerator.GenerateAppointment(
                professionalId: professionalId,
                patientId: patientId,
                slot: chosenSlot,
                description: "Tentativa de sobreposição."
            );

            // Act
            Func<Task> act = () => scheduleAppointmentUseCase.Handle(secondRequest);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Já existe um agendamento conflitante para o horário solicitado.");
        }

        [Fact]
        public async Task Nao_Deve_Permitir_Slot_Fora_Da_Politica()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            var professionalRepo = new ProfessionalRepository(context);
            var patientRepo = new PatientRepository(context);
            IProfessionalScheduleRepository scheduleRepo = new ProfessionalScheduleRepository(context);
            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);
            INotificationRepository notificationRepo = new NotificationRepository(context);
            IUserRepository userRepo = new UserRepository(context);
            var createNotificationUseCase = new CreateNotificationUseCase(notificationRepo, userRepo);

            // Profissional
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
            var professionalId = professionalResult.ProfessionalId;

            // Política: segunda, 08:00–10:00, duração 30min
            var schedule = await context.ProfessionalSchedules
                .Include(ps => ps.SchedulePolicy)
                    .ThenInclude(sp => sp.WeeklyWindows)
                .Include(ps => ps.Professional)
                .FirstAsync(ps => ps.Professional.Id == professionalId);

            schedule.SchedulePolicy.DurationInMinutes = 30;
            schedule.SchedulePolicy.WeeklyWindows.Clear();

            schedule.SchedulePolicy.WeeklyWindows.Add(new WeeklyWindow
            {
                DayOfWeek = WeekDay.Monday,
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(10, 0),
                SchedulePolicy = schedule.SchedulePolicy
            });

            // Paciente válido
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
            var patientId = patientResult.PatientId;

            // Slot fora da janela (segunda 07:00–07:30)
            DateTime baseDate = new DateTime(2025, 1, 6, 7, 0, 0); // segunda 07:00
            var invalidSlot = new ScheduleSlotDto
            {
                StartDateTime = baseDate,
                EndDateTime = baseDate.AddMinutes(30),
                Status = ScheduleSlotStatus.Available
            };

            var appointmentRequest = AppointmentGenerator.GenerateAppointment(
                professionalId: professionalId,
                patientId: patientId,
                slot: invalidSlot,
                description: "Slot fora da política."
            );

            var scheduleAppointmentUseCase = new ScheduleAppointmentUseCase(
                scheduleRepo,
                patientRepo,
                appointmentRepo,
                createNotificationUseCase);

            // Act
            Func<Task> act = () => scheduleAppointmentUseCase.Handle(appointmentRequest);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("O horário solicitado não está dentro da política de agendamento do profissional.");
        }

        [Fact]
        public async Task Nao_Deve_Permitir_Agendar_Com_Paciente_Inexistente()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            var professionalRepo = new ProfessionalRepository(context);
            var patientRepo = new PatientRepository(context);
            IProfessionalScheduleRepository scheduleRepo = new ProfessionalScheduleRepository(context);
            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);
            INotificationRepository notificationRepo = new NotificationRepository(context);
            IUserRepository userRepo = new UserRepository(context);
            var createNotificationUseCase = new CreateNotificationUseCase(notificationRepo, userRepo);

            // Profissional
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
            var professionalId = professionalResult.ProfessionalId;

            // Política determinística
            var schedule = await context.ProfessionalSchedules
                .Include(ps => ps.SchedulePolicy)
                    .ThenInclude(sp => sp.WeeklyWindows)
                .Include(ps => ps.Professional)
                .FirstAsync(ps => ps.Professional.Id == professionalId);

            schedule.SchedulePolicy.DurationInMinutes = 30;
            schedule.SchedulePolicy.WeeklyWindows.Clear();

            schedule.SchedulePolicy.WeeklyWindows.Add(new WeeklyWindow
            {
                DayOfWeek = WeekDay.Monday,
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(10, 0),
                SchedulePolicy = schedule.SchedulePolicy
            });

            // Slot válido dentro da política
            DateTime baseDate = new DateTime(2025, 1, 6, 8, 0, 0);
            var validSlot = new ScheduleSlotDto
            {
                StartDateTime = baseDate,
                EndDateTime = baseDate.AddMinutes(30),
                Status = ScheduleSlotStatus.Available
            };

            Guid nonExistingPatientId = Guid.NewGuid();

            var appointmentRequest = AppointmentGenerator.GenerateAppointment(
                professionalId: professionalId,
                patientId: nonExistingPatientId,
                slot: validSlot,
                description: "Paciente inexistente."
            );

            var scheduleAppointmentUseCase = new ScheduleAppointmentUseCase(
                scheduleRepo,
                patientRepo,
                appointmentRepo,
                createNotificationUseCase);

            // Act
            Func<Task> act = () => scheduleAppointmentUseCase.Handle(appointmentRequest);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Paciente informado não foi encontrado.");
        }
    }
}
