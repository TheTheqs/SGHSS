// Tests/Application/Patient/Register/RegisterPatientTests.cs


using FluentAssertions;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.Patients.Register;
using SGHSS.Domain.Enums;
using SGHSS.Domain.ValueObjects;
using SGHSS.Tests.Application.UseCases.Patients.Register;
using Xunit;

namespace SGHSS.Tests.Application.Patient.Register
{
    public class RegisterPatientTests
    {

        [Fact]
        public async Task Deve_Salvar_Novo_Paciente()
        {
            // Arrange
            RegisterPatientRequest _example = new RegisterPatientRequest
            {
                Name = "Ana Souza",
                Email = "ana_email@email.com",
                Phone = "19989256478",
                Cpf = "197.396.915-79",
                BirthDate = new DateTimeOffset(1990, 5, 20, 0, 0, 0, TimeSpan.Zero),
                Sex = Sex.Female,
                Address = new AddressDto
                {
                    Street = "Rua das Flores",
                    Number = "100",
                    City = "São Paulo",
                    State = "SP",
                    Cep = "01001000",
                    District = "Centro",
                    Complement = null
                },
                EmergencyContactName = "Gervásio Sousa",
            };
            var repo = new FakePatientRepository();
            var useCase = new RegisterPatientUseCase(repo);
            var request = _example;        
            var result = await useCase.Handle(request);
            var consult = await repo.ExistsByCpfAsync(new Cpf(request.Cpf));

            result.Should().NotBeNull();
            result.PatientId.Should().NotBe(Guid.Empty);
            consult.Should().BeTrue();
        }

        [Fact]
        public async Task Deve_Rejeitar_Cpf_Duplicado()
        {
            // Arrange
            RegisterPatientRequest _example = new RegisterPatientRequest
            {
                Name = "Ana Souza",
                Email = "ana_email@email.com",
                Phone = "19989256478",
                Cpf = "41019075953",
                BirthDate = new DateTimeOffset(1990, 5, 20, 0, 0, 0, TimeSpan.Zero),
                Sex = Sex.Female,
                Address = new AddressDto
                {
                    Street = "Rua das Flores",
                    Number = "100",
                    City = "São Paulo",
                    State = "SP",
                    Cep = "01001000",
                    District = "Centro",
                    Complement = null
                },
                EmergencyContactName = "Gervásio Sousa",
            };
            var repo = new FakePatientRepository();
            var useCase = new RegisterPatientUseCase(repo);
            var request = _example;
            Func<Task> act = () => useCase.Handle(request);
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Já existe um paciente cadastrado com o CPF informado.");
        }

        [Fact]
        public async Task Deve_Rejeitar_email_Duplicado()
        {
            // Arrange
            RegisterPatientRequest _example = new RegisterPatientRequest
            {
                Name = "Ana Souza",
                Email = "maria@example.com",
                Phone = "19989256478",
                Cpf = "197.396.915-79",
                BirthDate = new DateTimeOffset(1990, 5, 20, 0, 0, 0, TimeSpan.Zero),
                Sex = Sex.Female,
                Address = new AddressDto
                {
                    Street = "Rua das Flores",
                    Number = "100",
                    City = "São Paulo",
                    State = "SP",
                    Cep = "01001000",
                    District = "Centro",
                    Complement = null
                },
                EmergencyContactName = "Gervásio Sousa",
            };
            var repo = new FakePatientRepository();
            var useCase = new RegisterPatientUseCase(repo);
            var request = _example;
            Func<Task> act = () => useCase.Handle(request);
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Já existe um paciente cadastrado com o Email informado.");
        }
    }

}