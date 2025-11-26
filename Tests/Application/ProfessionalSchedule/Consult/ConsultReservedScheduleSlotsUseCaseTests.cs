// Tests/Application/ProfessionalSchedules/Consult/ConsultReservedScheduleSlotsUseCaseTests.cs

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.ProfessionalSchedules.Consult;
using SGHSS.Application.UseCases.Professionals.Register;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Professionals;
using Xunit;

namespace SGHSS.Tests.Application.ProfessionalSchedules.Consult
{
    public class ConsultReservedScheduleSlotsUseCaseTests
    {
        [Fact]
        public async Task Deve_Retornar_Apenas_Slots_Reservados_Quando_Agenda_Possuir_Slots_Com_Multiplos_Status()
        {
            // Arrange
            RegisterProfessionalRequest example =
                ProfessionalGenerator.GenerateProfessional(durationInMinutes: 30, weeklyWindowsCount: 0);

            var treatmentConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            var researchConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            var notificationConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);

            example.Consents.Add(treatmentConsent);
            example.Consents.Add(researchConsent);
            example.Consents.Add(notificationConsent);

            using var context = DbContextTestFactory.CreateInMemoryContext();

            var professionalRepo = new ProfessionalRepository(context);
            var registerUseCase = new RegisterProfessionalUseCase(professionalRepo);

            var registerResult = await registerUseCase.Handle(example);

            var schedule = await context.ProfessionalSchedules
                .Include(ps => ps.ScheduleSlots)
                .Include(ps => ps.Professional)
                .FirstAsync(ps => ps.Professional.Id == registerResult.ProfessionalId);

            DateTime baseDate = new DateTime(2025, 1, 6, 8, 0, 0);

            // Slot não reservado (status padrão)
            schedule.ScheduleSlots.Add(new ScheduleSlot
            {
                StartDateTime = baseDate,
                EndDateTime = baseDate.AddMinutes(30),
                Status = default,
                ProfessionalSchedule = schedule
            });

            // Slot reservado
            DateTime reservedStart = baseDate.AddMinutes(30);
            DateTime reservedEnd = reservedStart.AddMinutes(30);

            schedule.ScheduleSlots.Add(new ScheduleSlot
            {
                StartDateTime = reservedStart,
                EndDateTime = reservedEnd,
                Status = ScheduleSlotStatus.Reserved,
                ProfessionalSchedule = schedule
            });

            // Outro slot não reservado
            schedule.ScheduleSlots.Add(new ScheduleSlot
            {
                StartDateTime = baseDate.AddMinutes(60),
                EndDateTime = baseDate.AddMinutes(90),
                Status = default,
                ProfessionalSchedule = schedule
            });

            await context.SaveChangesAsync();

            IProfessionalScheduleRepository scheduleRepo = new ProfessionalScheduleRepository(context);
            var useCase = new ConsultReservedScheduleSlotsUseCase(scheduleRepo);

            var request = new ConsultReservedScheduleSlotsRequest
            {
                ProfessionalScheduleId = schedule.Id
            };

            // Act
            var response = await useCase.Handle(request);

            // Assert
            response.Should().NotBeNull();
            response.ProfessionalScheduleId.Should().Be(schedule.Id);

            response.ScheduleSlots.Should().HaveCount(1);

            response.ScheduleSlots.Should().OnlyContain(slot =>
                slot.Status == ScheduleSlotStatus.Reserved
            );

            // Se o DTO expõe StartDateTime/EndDateTime, validamos também os horários
            response.ScheduleSlots.Should().ContainSingle(slot =>
                slot.Status == ScheduleSlotStatus.Reserved
                && slot.StartDateTime == reservedStart
                && slot.EndDateTime == reservedEnd
            );
        }

        [Fact]
        public async Task Deve_Retornar_Colecao_Vazia_Quando_Nao_Houver_Slots_Reservados()
        {
            // Arrange
            RegisterProfessionalRequest example =
                ProfessionalGenerator.GenerateProfessional(durationInMinutes: 30, weeklyWindowsCount: 0);

            var treatmentConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            var researchConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            var notificationConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);

            example.Consents.Add(treatmentConsent);
            example.Consents.Add(researchConsent);
            example.Consents.Add(notificationConsent);

            using var context = DbContextTestFactory.CreateInMemoryContext();

            var professionalRepo = new ProfessionalRepository(context);
            var registerUseCase = new RegisterProfessionalUseCase(professionalRepo);

            var registerResult = await registerUseCase.Handle(example);

            var schedule = await context.ProfessionalSchedules
                .Include(ps => ps.ScheduleSlots)
                .Include(ps => ps.Professional)
                .FirstAsync(ps => ps.Professional.Id == registerResult.ProfessionalId);

            DateTime baseDate = new DateTime(2025, 1, 6, 8, 0, 0);

            // Apenas slots não reservados
            schedule.ScheduleSlots.Add(new ScheduleSlot
            {
                StartDateTime = baseDate,
                EndDateTime = baseDate.AddMinutes(30),
                Status = default,
                ProfessionalSchedule = schedule
            });

            schedule.ScheduleSlots.Add(new ScheduleSlot
            {
                StartDateTime = baseDate.AddMinutes(30),
                EndDateTime = baseDate.AddMinutes(60),
                Status = default,
                ProfessionalSchedule = schedule
            });

            await context.SaveChangesAsync();

            IProfessionalScheduleRepository scheduleRepo = new ProfessionalScheduleRepository(context);
            var useCase = new ConsultReservedScheduleSlotsUseCase(scheduleRepo);

            var request = new ConsultReservedScheduleSlotsRequest
            {
                ProfessionalScheduleId = schedule.Id
            };

            // Act
            var response = await useCase.Handle(request);

            // Assert
            response.Should().NotBeNull();
            response.ProfessionalScheduleId.Should().Be(schedule.Id);
            response.ScheduleSlots.Should().BeEmpty();
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Agenda_Nao_For_Encontrada()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IProfessionalScheduleRepository scheduleRepo = new ProfessionalScheduleRepository(context);
            var useCase = new ConsultReservedScheduleSlotsUseCase(scheduleRepo);

            var request = new ConsultReservedScheduleSlotsRequest
            {
                ProfessionalScheduleId = Guid.NewGuid()
            };

            // Act
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Não foi possível localizar uma agenda profissional para o identificador informado.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Identificador_De_Agenda_For_Vazio()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IProfessionalScheduleRepository scheduleRepo = new ProfessionalScheduleRepository(context);
            var useCase = new ConsultReservedScheduleSlotsUseCase(scheduleRepo);

            var request = new ConsultReservedScheduleSlotsRequest
            {
                ProfessionalScheduleId = Guid.Empty
            };

            // Act
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("O identificador da agenda profissional não pode ser vazio.");
        }
    }
}
