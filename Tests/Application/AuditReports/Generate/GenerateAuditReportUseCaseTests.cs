// Tests/Application/AuditReports/Generate/GenerateAuditReportUseCaseTests.cs

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.AuditReports.Generate;
using SGHSS.Application.UseCases.Common;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Models;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Administrators;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Common;
using Xunit;

namespace SGHSS.Tests.Application.AuditReports.Generate
{
    public class GenerateAuditReportUseCaseTests
    {
        [Fact]
        public async Task Deve_Gerar_Relatorio_De_Auditoria_Quando_Existirem_Logs_No_Periodo()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IAuditReportRepository auditReportRepo = new AuditReportRepository(context);
            ILogActivityRepository logActivityRepo = new LogActivityRepository(context);
            IAdministratorRepository administratorRepo = new AdministratorRepository(context);
            IUserRepository userRepo = new UserRepository(context);

            // ===== Cria Administrator válido =====
            RegisterAdministratorRequest adminExample = AdministratorGenerator.GenerateAdministrator();

            ConsentDto treatmentConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.App, true);
            ConsentDto notificationConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.App, true);

            adminExample.Consents.Add(treatmentConsent);
            adminExample.Consents.Add(notificationConsent);

            var registerAdminUseCase = new RegisterAdministratorUseCase(administratorRepo);
            var adminResult = await registerAdminUseCase.Handle(adminExample);

            adminResult.Should().NotBeNull();
            adminResult.AdministratorId.Should().NotBe(Guid.Empty);

            var administrator = await context.Administrators
                .FirstAsync(a => a.Id == adminResult.AdministratorId);

            Guid administratorId = administrator.Id;

            // ===== Cria alguns logs no período e fora do período =====
            DateTimeOffset baseDate = new DateTimeOffset(2025, 1, 6, 8, 0, 0, TimeSpan.Zero);
            DateTimeOffset from = baseDate;
            DateTimeOffset to = baseDate.AddHours(1); // 08:00–09:00

            // Log anterior ao período (não deve entrar)
            var logBefore = new LogActivity
            {
                Timestamp = baseDate.AddMinutes(-30),
                Action = "Login",
                Description = "User logged in before range.",
                IpAddress = new IpAddress("192.168.0.1"),
                Result = LogResult.Success,
                User = administrator
            };

            // Log dentro do período
            var logInside1 = new LogActivity
            {
                Timestamp = baseDate.AddMinutes(10),
                Action = "RegisterPatient",
                Description = "Patient successfully registered.",
                IpAddress = new IpAddress("192.168.0.2"),
                Result = LogResult.Success,
                User = administrator
            };

            // Outro log dentro do período
            var logInside2 = new LogActivity
            {
                Timestamp = baseDate.AddMinutes(40),
                Action = "UpdateBedStatus",
                Description = "Bed marked as under maintenance.",
                IpAddress = new IpAddress("192.168.0.3"),
                Result = LogResult.Warning,
                User = administrator
            };

            await logActivityRepo.AddAsync(logBefore);
            await logActivityRepo.AddAsync(logInside1);
            await logActivityRepo.AddAsync(logInside2);

            var useCase = new GenerateAuditReportUseCase(
                auditReportRepo,
                logActivityRepo,
                administratorRepo);

            var request = new GenerateAuditReportRequest
            {
                AdministratorId = administratorId,
                From = from,
                To = to
            };

            // Act
            var response = await useCase.Handle(request);

            // Assert (response)
            response.Should().NotBeNull();
            response.AuditReportId.Should().NotBe(Guid.Empty);
            response.AdministratorId.Should().Be(administratorId);
            response.CreatedAt.Should().NotBe(default);
            response.From.Should().Be(from);
            response.To.Should().Be(to);

            response.ReportDetails.Should().NotBeNullOrWhiteSpace();
            response.ReportDetails.Should().Contain("AUDIT REPORT");
            response.ReportDetails.Should().Contain("TotalEntries:  2");
            response.ReportDetails.Should().Contain("RegisterPatient");
            response.ReportDetails.Should().Contain("UpdateBedStatus");
            response.ReportDetails.Should().NotContain("User logged in before range.");

            // Assert (banco)
            var persistedReport = await context.AuditReports
                .Include(ar => ar.CreatedBy)
                .FirstAsync(ar => ar.Id == response.AuditReportId);

            persistedReport.Should().NotBeNull();
            persistedReport.CreatedBy.Should().NotBeNull();
            persistedReport.CreatedBy.Id.Should().Be(administratorId);

            persistedReport.ReportDetails.Should().Be(response.ReportDetails);
            persistedReport.CreatedAt.Should().Be(response.CreatedAt);
        }

        [Fact]
        public async Task Deve_Gerar_Relatorio_Vazio_Quando_Nao_Houver_Logs_No_Periodo()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IAuditReportRepository auditReportRepo = new AuditReportRepository(context);
            ILogActivityRepository logActivityRepo = new LogActivityRepository(context);
            IAdministratorRepository administratorRepo = new AdministratorRepository(context);

            // ===== Cria Administrator válido =====
            RegisterAdministratorRequest adminExample = AdministratorGenerator.GenerateAdministrator();

            ConsentDto treatmentConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.App, true);
            ConsentDto notificationConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.App, true);

            adminExample.Consents.Add(treatmentConsent);
            adminExample.Consents.Add(notificationConsent);

            var registerAdminUseCase = new RegisterAdministratorUseCase(administratorRepo);
            var adminResult = await registerAdminUseCase.Handle(adminExample);

            adminResult.Should().NotBeNull();
            adminResult.AdministratorId.Should().NotBe(Guid.Empty);

            Guid administratorId = adminResult.AdministratorId;

            DateTimeOffset from = new DateTimeOffset(2025, 1, 6, 8, 0, 0, TimeSpan.Zero);
            DateTimeOffset to = from.AddHours(1);

            var useCase = new GenerateAuditReportUseCase(
                auditReportRepo,
                logActivityRepo,
                administratorRepo);

            var request = new GenerateAuditReportRequest
            {
                AdministratorId = administratorId,
                From = from,
                To = to
            };

            // Act
            var response = await useCase.Handle(request);

            // Assert
            response.Should().NotBeNull();
            response.AuditReportId.Should().NotBe(Guid.Empty);
            response.ReportDetails.Should().Contain("TotalEntries:  0");
            response.ReportDetails.Should().Contain("Nenhum registro de atividade foi encontrado para o período informado.");

            var persistedReport = await context.AuditReports
                .Include(ar => ar.CreatedBy)
                .FirstAsync(ar => ar.Id == response.AuditReportId);

            persistedReport.Should().NotBeNull();
            persistedReport.CreatedBy.Id.Should().Be(administratorId);
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Administrador_Nao_For_Encontrado()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IAuditReportRepository auditReportRepo = new AuditReportRepository(context);
            ILogActivityRepository logActivityRepo = new LogActivityRepository(context);
            IAdministratorRepository administratorRepo = new AdministratorRepository(context);

            var useCase = new GenerateAuditReportUseCase(
                auditReportRepo,
                logActivityRepo,
                administratorRepo);

            DateTimeOffset from = new DateTimeOffset(2025, 1, 6, 8, 0, 0, TimeSpan.Zero);
            DateTimeOffset to = from.AddHours(1);

            var request = new GenerateAuditReportRequest
            {
                AdministratorId = Guid.NewGuid(), // inexistente
                From = from,
                To = to
            };

            // Act
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Não foi possível localizar um administrador para o identificador informado.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Intervalo_For_Invalido()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IAuditReportRepository auditReportRepo = new AuditReportRepository(context);
            ILogActivityRepository logActivityRepo = new LogActivityRepository(context);
            IAdministratorRepository administratorRepo = new AdministratorRepository(context);

            var useCase = new GenerateAuditReportUseCase(
                auditReportRepo,
                logActivityRepo,
                administratorRepo);

            DateTimeOffset from = new DateTimeOffset(2025, 1, 6, 8, 0, 0, TimeSpan.Zero);
            DateTimeOffset to = from; // inválido (From == To)

            var request = new GenerateAuditReportRequest
            {
                AdministratorId = Guid.NewGuid(), // nem chega a consultar
                From = from,
                To = to
            };

            // Act
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("O intervalo de datas informado é inválido. A data inicial deve ser menor que a final.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Request_For_Nulo()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IAuditReportRepository auditReportRepo = new AuditReportRepository(context);
            ILogActivityRepository logActivityRepo = new LogActivityRepository(context);
            IAdministratorRepository administratorRepo = new AdministratorRepository(context);

            var useCase = new GenerateAuditReportUseCase(
                auditReportRepo,
                logActivityRepo,
                administratorRepo);

            // Act
            Func<Task> act = () => useCase.Handle(null!);

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentNullException>()
                .WithMessage("A requisição não pode ser nula.*");
        }
    }
}
