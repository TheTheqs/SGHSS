// Tests/Application/ProfessionalSchedules/Consult/GenerateAvailableSlotsUseCaseTests.cs

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Common;
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
    public class GenerateAvailableSlotsUseCaseTests
    {
        [Fact]
        public async Task Deve_Gerar_Slots_Disponiveis_Quando_Nao_Ha_Slots_Ocupados()
        {
            // Arrange
            RegisterProfessionalRequest example =
                ProfessionalGenerator.GenerateProfessional(durationInMinutes: 30, weeklyWindowsCount: 0);

            ConsentDto treatmentConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);

            example.Consents.Add(treatmentConsent);
            example.Consents.Add(researchConsent);
            example.Consents.Add(notificationConsent);

            using var context = DbContextTestFactory.CreateInMemoryContext();

            var professionalRepo = new ProfessionalRepository(context);
            var registerUseCase = new RegisterProfessionalUseCase(professionalRepo);

            var registerResult = await registerUseCase.Handle(example);

            var schedule = await context.ProfessionalSchedules
                .Include(ps => ps.SchedulePolicy)
                    .ThenInclude(sp => sp.WeeklyWindows)
                .Include(ps => ps.Professional)
                .FirstAsync(ps => ps.Professional.Id == registerResult.ProfessionalId);

            schedule.SchedulePolicy.DurationInMinutes = 30;
            schedule.SchedulePolicy.WeeklyWindows.Clear();

            schedule.SchedulePolicy.WeeklyWindows.Add(new WeeklyWindow
            {
                DayOfWeek = WeekDay.Monday,
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(10, 0),
                SchedulePolicy = schedule.SchedulePolicy
            });

            await context.SaveChangesAsync();

            IProfessionalScheduleRepository scheduleRepo = new ProfessionalScheduleRepository(context);
            var availabilityUseCase = new GenerateAvailableSlotsUseCase(scheduleRepo);

            DateTime from = new DateTime(2025, 1, 6, 8, 0, 0);   // segunda-feira
            DateTime to = new DateTime(2025, 1, 6, 10, 0, 0);    // mesmo dia, 2h depois

            var request = new GenerateAvailableSlotsRequest
            {
                ProfessionalId = registerResult.ProfessionalId,
                From = from,
                To = to
            };

            // Act
            var response = await availabilityUseCase.Handle(request);

            // Assert
            response.Should().NotBeNull();
            response.ProfessionalId.Should().Be(registerResult.ProfessionalId);

            response.Slots.Should().HaveCount(4);

            response.Slots.Should().BeEquivalentTo(
                new[]
                {
                    new { StartDateTime = from,                         EndDateTime = from.AddMinutes(30)  },
                    new { StartDateTime = from.AddMinutes(30),          EndDateTime = from.AddMinutes(60)  },
                    new { StartDateTime = from.AddMinutes(60),          EndDateTime = from.AddMinutes(90)  },
                    new { StartDateTime = from.AddMinutes(90),          EndDateTime = from.AddMinutes(120) }
                },
                options => options.WithStrictOrdering()
            );
        }

        [Fact]
        public async Task Deve_Remover_Slot_Conflitante_Quando_Ja_Existe_Slot_Ocupado()
        {
            // Arrange
            RegisterProfessionalRequest example =
                ProfessionalGenerator.GenerateProfessional(durationInMinutes: 30, weeklyWindowsCount: 0);

            ConsentDto treatmentConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);

            example.Consents.Add(treatmentConsent);
            example.Consents.Add(researchConsent);
            example.Consents.Add(notificationConsent);

            using var context = DbContextTestFactory.CreateInMemoryContext();

            var professionalRepo = new ProfessionalRepository(context);
            var registerUseCase = new RegisterProfessionalUseCase(professionalRepo);

            var registerResult = await registerUseCase.Handle(example);

            var schedule = await context.ProfessionalSchedules
                .Include(ps => ps.SchedulePolicy)
                    .ThenInclude(sp => sp.WeeklyWindows)
                .Include(ps => ps.ScheduleSlots)
                .Include(ps => ps.Professional)
                .FirstAsync(ps => ps.Professional.Id == registerResult.ProfessionalId);

            schedule.SchedulePolicy.DurationInMinutes = 30;
            schedule.SchedulePolicy.WeeklyWindows.Clear();

            schedule.SchedulePolicy.WeeklyWindows.Add(new WeeklyWindow
            {
                DayOfWeek = WeekDay.Monday,
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(10, 0),
                SchedulePolicy = schedule.SchedulePolicy
            });

            DateTime baseDate = new DateTime(2025, 1, 6, 8, 0, 0); // segunda
            DateTime conflictStart = baseDate.AddMinutes(30);      // 08:30
            DateTime conflictEnd = conflictStart.AddMinutes(30);   // 09:00

            schedule.ScheduleSlots.Add(new ScheduleSlot
            {
                StartDateTime = conflictStart,
                EndDateTime = conflictEnd,
                Status = default, // valor padrão, Status não é relevante para o cálculo
                ProfessionalSchedule = schedule
            });

            await context.SaveChangesAsync();

            IProfessionalScheduleRepository scheduleRepo = new ProfessionalScheduleRepository(context);
            var availabilityUseCase = new GenerateAvailableSlotsUseCase(scheduleRepo);

            DateTime from = baseDate;
            DateTime to = baseDate.AddHours(2); // 08:00–10:00

            var request = new GenerateAvailableSlotsRequest
            {
                ProfessionalId = registerResult.ProfessionalId,
                From = from,
                To = to
            };

            // Act
            var response = await availabilityUseCase.Handle(request);

            // Assert
            response.Slots.Should().HaveCount(3);

            response.Slots.Should().NotContain(slot =>
                slot.StartDateTime == conflictStart &&
                slot.EndDateTime == conflictEnd);

            response.Slots.Should().BeEquivalentTo(
                new[]
                {
                    new { StartDateTime = from,                         EndDateTime = from.AddMinutes(30)  },
                    new { StartDateTime = from.AddMinutes(60),          EndDateTime = from.AddMinutes(90)  },
                    new { StartDateTime = from.AddMinutes(90),          EndDateTime = from.AddMinutes(120) }
                },
                options => options.WithoutStrictOrdering()
            );
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Nao_Existir_Agenda_Para_Profissional()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IProfessionalScheduleRepository scheduleRepo = new ProfessionalScheduleRepository(context);
            var availabilityUseCase = new GenerateAvailableSlotsUseCase(scheduleRepo);

            var request = new GenerateAvailableSlotsRequest
            {
                ProfessionalId = Guid.NewGuid(),
                From = new DateTime(2025, 1, 6, 8, 0, 0),
                To = new DateTime(2025, 1, 6, 10, 0, 0)
            };

            // Act
            Func<Task> act = () => availabilityUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Nenhuma agenda configurada foi encontrada para o profissional informado.");
        }
    }
}
