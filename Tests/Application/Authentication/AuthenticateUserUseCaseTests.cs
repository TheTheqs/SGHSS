// Tests/Application/Authentication/AuthenticateUserUseCaseTests.cs

using FluentAssertions;
using SGHSS.Application.Interfaces.Services;
using SGHSS.Application.UseCases.Authentication;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.Patients.Register;
using SGHSS.Application.UseCases.Professionals.Register;
using SGHSS.Domain.Enums;
using SGHSS.Domain.ValueObjects;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Administrators;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Patients;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Professionals;
using Xunit;

namespace SGHSS.Tests.Application.Authentication
{
    public class AuthenticateUserUseCaseTests
    {
        /// <summary>
        /// Implementação fake de ITokenService apenas para fins de teste.
        /// Retorna sempre o mesmo token e define expiração relativa.
        /// </summary>
        private sealed class FakeTokenService : ITokenService
        {
            public (string Token, DateTimeOffset ExpiresAt) GenerateToken(
                SGHSS.Domain.Models.User user,
                AccessLevel accessLevel)
            {
                return ("fake-token", DateTimeOffset.UtcNow.AddHours(1));
            }
        }

        [Fact]
        public async Task Deve_Autenticar_Administrador_Quando_Credenciais_Forem_Validas()
        {
            // Arrange
            var adminRequest = AdministratorGenerator.GenerateAdministrator();

            // Adiciona consentimentos obrigatórios
            var treatmentConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            var researchConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            var notificationConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);

            adminRequest.Consents.Add(treatmentConsent);
            adminRequest.Consents.Add(researchConsent);
            adminRequest.Consents.Add(notificationConsent);

            using var context = DbContextTestFactory.CreateInMemoryContext();

            // Persiste o administrador usando o caso de uso de registro
            var adminRepo = new AdministratorRepository(context);
            var registerAdminUseCase = new RegisterAdministratorUseCase(adminRepo);

            var registerResult = await registerAdminUseCase.Handle(adminRequest);
            registerResult.Should().NotBeNull();
            registerResult.AdministratorId.Should().NotBe(Guid.Empty);

            // Repositório de usuários genérico + TokenService fake
            var userRepo = new UserRepository(context);
            var tokenService = new FakeTokenService();

            var authenticateUseCase = new AuthenticateUserUseCase(userRepo, tokenService);

            var authRequest = new AuthenticateUserRequest
            {
                Email = adminRequest.Email,
                Password = adminRequest.Password
            };

            // Act
            var authResponse = await authenticateUseCase.Handle(authRequest);

            // Assert
            authResponse.Should().NotBeNull();
            authResponse.UserId.Should().Be(registerResult.AdministratorId);
            authResponse.Email.Should().Be(adminRequest.Email);
            authResponse.Name.Should().Be(adminRequest.Name);
            authResponse.UserType.Should().Be(nameof(SGHSS.Domain.Models.Administrator));
            authResponse.AccessLevel.Should().Be(adminRequest.AccessLevel);
            authResponse.Token.Should().Be("fake-token");
            authResponse.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);
        }

        [Fact]
        public async Task Deve_Autenticar_Paciente_Quando_Credenciais_Forem_Validas()
        {
            // Arrange
            var patientRequest = PatientGenerator.GeneratePatient();

            // Adiciona consentimentos obrigatórios
            var treatmentConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            var researchConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            var notificationConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);

            patientRequest.Consents.Add(treatmentConsent);
            patientRequest.Consents.Add(researchConsent);
            patientRequest.Consents.Add(notificationConsent);

            using var context = DbContextTestFactory.CreateInMemoryContext();

            // Persiste o paciente usando o caso de uso de registro
            var patientRepo = new PatientRepository(context);
            var registerPatientUseCase = new RegisterPatientUseCase(patientRepo);

            var registerResult = await registerPatientUseCase.Handle(patientRequest);
            registerResult.Should().NotBeNull();
            registerResult.PatientId.Should().NotBe(Guid.Empty);

            // Repositório de usuários genérico + TokenService fake
            var userRepo = new UserRepository(context);
            var tokenService = new FakeTokenService();

            var authenticateUseCase = new AuthenticateUserUseCase(userRepo, tokenService);

            var authRequest = new AuthenticateUserRequest
            {
                Email = patientRequest.Email,
                Password = patientRequest.Password
            };

            // Act
            var authResponse = await authenticateUseCase.Handle(authRequest);

            // Assert
            authResponse.Should().NotBeNull();
            authResponse.UserId.Should().Be(registerResult.PatientId);
            authResponse.Email.Should().Be(patientRequest.Email);
            authResponse.Name.Should().Be(patientRequest.Name);
            authResponse.UserType.Should().Be(nameof(SGHSS.Domain.Models.Patient));
            authResponse.AccessLevel.Should().Be(AccessLevel.Patient);
            authResponse.Token.Should().Be("fake-token");
            authResponse.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);
        }

        [Fact]
        public async Task Deve_Autenticar_Profissional_Quando_Credenciais_Forem_Validas()
        {
            // Arrange
            var professionalRequest =
                ProfessionalGenerator.GenerateProfessional(durationInMinutes: 30, weeklyWindowsCount: 2);

            // Adiciona consentimentos obrigatórios
            var treatmentConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            var researchConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            var notificationConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);

            professionalRequest.Consents.Add(treatmentConsent);
            professionalRequest.Consents.Add(researchConsent);
            professionalRequest.Consents.Add(notificationConsent);

            using var context = DbContextTestFactory.CreateInMemoryContext();

            // Persiste o profissional usando o caso de uso de registro
            var professionalRepo = new ProfessionalRepository(context);
            var registerProfessionalUseCase = new RegisterProfessionalUseCase(professionalRepo);

            var registerResult = await registerProfessionalUseCase.Handle(professionalRequest);
            registerResult.Should().NotBeNull();
            registerResult.ProfessionalId.Should().NotBe(Guid.Empty);

            // Repositório de usuários genérico + TokenService fake
            var userRepo = new UserRepository(context);
            var tokenService = new FakeTokenService();

            var authenticateUseCase = new AuthenticateUserUseCase(userRepo, tokenService);

            var authRequest = new AuthenticateUserRequest
            {
                Email = professionalRequest.Email,
                Password = professionalRequest.Password
            };

            // Act
            var authResponse = await authenticateUseCase.Handle(authRequest);

            // Assert
            authResponse.Should().NotBeNull();
            authResponse.UserId.Should().Be(registerResult.ProfessionalId);
            authResponse.Email.Should().Be(professionalRequest.Email);
            authResponse.Name.Should().Be(professionalRequest.Name);
            authResponse.UserType.Should().Be(nameof(SGHSS.Domain.Models.Professional));
            authResponse.AccessLevel.Should().Be(AccessLevel.Professional);
            authResponse.Token.Should().Be("fake-token");
            authResponse.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);
        }

        [Fact]
        public async Task Deve_Recusar_Quando_Senha_For_Invalida()
        {
            // Arrange
            var adminRequest = AdministratorGenerator.GenerateAdministrator();

            // Consentimentos obrigatórios
            var treatmentConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.Web, true);
            var researchConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Research, ConsentChannel.Web, true);
            var notificationConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.Web, true);

            adminRequest.Consents.Add(treatmentConsent);
            adminRequest.Consents.Add(researchConsent);
            adminRequest.Consents.Add(notificationConsent);

            using var context = DbContextTestFactory.CreateInMemoryContext();

            // Persiste o administrador normalmente
            var adminRepo = new AdministratorRepository(context);
            var registerAdminUseCase = new RegisterAdministratorUseCase(adminRepo);

            var registerResult = await registerAdminUseCase.Handle(adminRequest);
            registerResult.Should().NotBeNull();
            registerResult.AdministratorId.Should().NotBe(Guid.Empty);

            var userRepo = new UserRepository(context);
            var tokenService = new FakeTokenService();

            var authenticateUseCase = new AuthenticateUserUseCase(userRepo, tokenService);

            var wrongPasswordRequest = new AuthenticateUserRequest
            {
                Email = adminRequest.Email,
                Password = "SenhaIncorreta123"
            };

            // Act
            Func<Task> act = () => authenticateUseCase.Handle(wrongPasswordRequest);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Credenciais inválidas.");
        }
    }
}
