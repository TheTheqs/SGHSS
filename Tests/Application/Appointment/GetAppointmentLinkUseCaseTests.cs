// Tests/Application/Appointment/GetAppointmentLinkUseCaseTests.cs

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Appointments.GetLink;
using SGHSS.Application.UseCases.Appointments.Register;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.ProfessionalSchedules.Consult;
using SGHSS.Application.UseCases.Professionals.Register;
using SGHSS.Application.UseCases.Patients.Register;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Appointments;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Patients;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Professionals;
using Xunit;

namespace SGHSS.Tests.Application.Appointments.Read
{
    public class GetAppointmentLinkUseCaseTests
    {
        [Fact]
        public async Task Deve_Retornar_Link_Quando_Agendamento_Possuir_Link()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            var professionalRepo = new ProfessionalRepository(context);
            var patientRepo = new PatientRepository(context);
            IProfessionalScheduleRepository scheduleRepo = new ProfessionalScheduleRepository(context);
            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);

            // ===== Profissional =====
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

            // Política de agenda determinística
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

            // ===== Paciente =====
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

            // ===== Slots disponíveis =====
            var generateSlotsUseCase = new GenerateAvailableSlotsUseCase(scheduleRepo);

            DateTime baseDate = new DateTime(2025, 1, 6, 8, 0, 0); // segunda-feira
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

            // ===== Agendamento da consulta =====
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
                appointmentRepo);

            var scheduleResponse = await scheduleAppointmentUseCase.Handle(appointmentRequest);
            scheduleResponse.Should().NotBeNull();

            // Recupera o agendamento salvo (com link)
            var savedAppointment = await context.Appointments
                .Include(a => a.ScheduleSlot)
                    .ThenInclude(ss => ss.ProfessionalSchedule)
                        .ThenInclude(ps => ps.Professional)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync();

            savedAppointment.Should().NotBeNull();
            savedAppointment!.Link.Should().NotBeNull();

            var useCase = new GetAppointmentLinkUseCase(appointmentRepo);

            var request = new GetAppointmentLinkRequest
            {
                AppointmentId = savedAppointment.Id
            };

            // Act
            var response = await useCase.Handle(request);

            // Assert
            response.Should().NotBeNull();
            response.Link.Should().NotBeNullOrWhiteSpace();
            response.Link.Should().Be(savedAppointment.Link.ToString());
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Agendamento_Nao_For_Encontrado()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IAppointmentRepository appointmentRepo = new AppointmentRepository(context);
            var useCase = new GetAppointmentLinkUseCase(appointmentRepo);

            var request = new GetAppointmentLinkRequest
            {
                AppointmentId = Guid.NewGuid()
            };

            // Act
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage($"Agendamento com ID {request.AppointmentId} não foi encontrado.");
        }
    }
}
