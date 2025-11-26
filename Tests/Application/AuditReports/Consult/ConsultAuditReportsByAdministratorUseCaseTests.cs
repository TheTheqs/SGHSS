// Tests/Application/AuditReports/Consult/ConsultAuditReportsByAdministratorUseCaseTests.cs

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.AuditReports.Consult;
using SGHSS.Application.UseCases.Common;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Administrators;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Common;
using Xunit;

namespace SGHSS.Tests.Application.AuditReports.Consult
{
    public class ConsultAuditReportsByAdministratorUseCaseTests
    {
        [Fact]
        public async Task Deve_Retornar_Relatorios_Quando_Existirem_Para_Administrador()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IAuditReportRepository auditReportRepo = new AuditReportRepository(context);
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

            var administrator = await context.Administrators
                .FirstAsync(a => a.Id == adminResult.AdministratorId);

            Guid administratorId = administrator.Id;

            // ===== Cria alguns relatórios para esse administrador =====
            var createdAt1 = new DateTimeOffset(2025, 1, 6, 8, 0, 0, TimeSpan.Zero);
            var createdAt2 = new DateTimeOffset(2025, 1, 7, 9, 30, 0, TimeSpan.Zero); // mais recente

            var report1 = new AuditReport
            {
                CreatedAt = createdAt1,
                ReportDetails = "REPORT 1 - Primeiro relatório de auditoria.",
                CreatedBy = administrator
            };

            var report2 = new AuditReport
            {
                CreatedAt = createdAt2,
                ReportDetails = "REPORT 2 - Segundo relatório de auditoria (mais recente).",
                CreatedBy = administrator
            };

            await auditReportRepo.AddAsync(report1);
            await auditReportRepo.AddAsync(report2);

            var useCase = new ConsultAuditReportsByAdministratorUseCase(
                auditReportRepo,
                administratorRepo);

            var request = new ConsultAuditReportsByAdministratorRequest
            {
                AdministratorId = administratorId
            };

            // Act
            var response = await useCase.Handle(request);

            // Assert
            response.Should().NotBeNull();
            response.AdministratorId.Should().Be(administratorId);

            response.Reports.Should().NotBeNull();
            response.Reports.Should().HaveCount(2);

            var reportsList = response.Reports.ToList();

            // Deve vir ordenado por CreatedAt decrescente (mais recente primeiro)
            reportsList[0].CreatedAt.Should().Be(createdAt2);
            reportsList[1].CreatedAt.Should().Be(createdAt1);

            reportsList[0].AuditReportId.Should().NotBe(Guid.Empty);
            reportsList[1].AuditReportId.Should().NotBe(Guid.Empty);

            // Preview deve conter parte do conteúdo original
            reportsList[0].Preview.Should().Contain("REPORT 2");
            reportsList[1].Preview.Should().Contain("REPORT 1");
        }

        [Fact]
        public async Task Deve_Retornar_Colecao_Vazia_Quando_Nao_Houver_Relatorios_Para_Administrador()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IAuditReportRepository auditReportRepo = new AuditReportRepository(context);
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

            var useCase = new ConsultAuditReportsByAdministratorUseCase(
                auditReportRepo,
                administratorRepo);

            var request = new ConsultAuditReportsByAdministratorRequest
            {
                AdministratorId = administratorId
            };

            // Act
            var response = await useCase.Handle(request);

            // Assert
            response.Should().NotBeNull();
            response.AdministratorId.Should().Be(administratorId);
            response.Reports.Should().NotBeNull();
            response.Reports.Should().BeEmpty();
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Administrador_Nao_For_Encontrado()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IAuditReportRepository auditReportRepo = new AuditReportRepository(context);
            IAdministratorRepository administratorRepo = new AdministratorRepository(context);

            var useCase = new ConsultAuditReportsByAdministratorUseCase(
                auditReportRepo,
                administratorRepo);

            var request = new ConsultAuditReportsByAdministratorRequest
            {
                AdministratorId = Guid.NewGuid() // não existe no banco
            };

            // Act
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Não foi possível localizar um administrador para o identificador informado.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Identificador_De_Administrador_For_Vazio()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IAuditReportRepository auditReportRepo = new AuditReportRepository(context);
            IAdministratorRepository administratorRepo = new AdministratorRepository(context);

            var useCase = new ConsultAuditReportsByAdministratorUseCase(
                auditReportRepo,
                administratorRepo);

            var request = new ConsultAuditReportsByAdministratorRequest
            {
                AdministratorId = Guid.Empty
            };

            // Act
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("O identificador do administrador não pode ser vazio.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Request_For_Nulo()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IAuditReportRepository auditReportRepo = new AuditReportRepository(context);
            IAdministratorRepository administratorRepo = new AdministratorRepository(context);

            var useCase = new ConsultAuditReportsByAdministratorUseCase(
                auditReportRepo,
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
