// Tests/Application/Professional/Update/UpdateProfessionalSchedulePolicyUseCaseTests.cs

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.Professionals.Register;
using SGHSS.Application.UseCases.Professionals.Update;
using SGHSS.Domain.Enums;
using SGHSS.Domain.ValueObjects;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Professionals;
using Xunit;

namespace SGHSS.Tests.Application.Professional.Update
{
    public class UpdateProfessionalSchedulePolicyUseCaseTests
    {
        [Fact]
        public async Task Deve_Atualizar_Politica_De_Agendamento_Quando_Dados_Forem_Validos()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IProfessionalRepository repo = new ProfessionalRepository(context);

            // 1) Cria um profissional válido com agenda inicial
            RegisterProfessionalRequest example =
                ProfessionalGenerator.GenerateProfessional(durationInMinutes: 30, weeklyWindowsCount: 2);

            ConsentDto treatmentConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            ConsentDto researchConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            ConsentDto notificationConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);

            example.Consents.Add(treatmentConsent);
            example.Consents.Add(researchConsent);
            example.Consents.Add(notificationConsent);

            var registerUseCase = new RegisterProfessionalUseCase(repo);
            var registerResult = await registerUseCase.Handle(example);

            // Garante que foi salvo
            registerResult.Should().NotBeNull();
            registerResult.ProfessionalId.Should().NotBe(Guid.Empty);

            // 2) Monta uma nova policy usando as mesmas weekly windows,
            // apenas alterando a duração e (se quiser) o fuso
            var newPolicy = new SchedulePolicyDto
            {
                DurationInMinutes = example.SchedulePolicy.DurationInMinutes + 15,
                TimeZone = example.SchedulePolicy.TimeZone,
                WeeklyWindows = example.SchedulePolicy.WeeklyWindows
            };

            var updateUseCase = new UpdateProfessionalSchedulePolicyUseCase(repo);

            var request = new UpdateProfessionalSchedulePolicyRequest
            {
                ProfessionalId = registerResult.ProfessionalId,
                SchedulePolicy = newPolicy
            };

            // Act
            var response = await updateUseCase.Handle(request);

            // Assert (response)
            response.Should().NotBeNull();
            response.ProfessionalId.Should().Be(registerResult.ProfessionalId);
            response.SchedulePolicy.Should().NotBeNull();
            response.SchedulePolicy.DurationInMinutes.Should().Be(newPolicy.DurationInMinutes);
            response.SchedulePolicy.TimeZone.Should().Be(newPolicy.TimeZone);
            response.SchedulePolicy.WeeklyWindows.Should().HaveSameCount(newPolicy.WeeklyWindows);

            // Assert (banco) – garante que foi persistido
            var reloadedProfessional = await context.Professionals
                .Include(p => p.ProfessionalSchedule)
                    .ThenInclude(ps => ps.SchedulePolicy)
                .FirstAsync(p => p.Id == registerResult.ProfessionalId);

            reloadedProfessional.ProfessionalSchedule.Should().NotBeNull();
            reloadedProfessional.ProfessionalSchedule.SchedulePolicy.Should().NotBeNull();
            reloadedProfessional
                .ProfessionalSchedule
                .SchedulePolicy
                .DurationInMinutes
                .Should()
                .Be(newPolicy.DurationInMinutes);
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Profissional_Nao_For_Encontrado()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IProfessionalRepository repo = new ProfessionalRepository(context);

            // Usa o generator só para obter uma policy DTO válida
            RegisterProfessionalRequest example =
                ProfessionalGenerator.GenerateProfessional(durationInMinutes: 30, weeklyWindowsCount: 2);

            var validPolicy = example.SchedulePolicy;

            var updateUseCase = new UpdateProfessionalSchedulePolicyUseCase(repo);

            var request = new UpdateProfessionalSchedulePolicyRequest
            {
                ProfessionalId = Guid.NewGuid(), // inexistente
                SchedulePolicy = validPolicy
            };

            // Act
            Func<Task> act = () => updateUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Profissional não encontrado para o identificador informado.");
        }
    }
}
