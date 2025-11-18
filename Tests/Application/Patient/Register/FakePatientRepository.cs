// Tests/Application/UseCases/Patients/Register/Fakes/FakePatientRepository.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Enums;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Tests.Application.UseCases.Patients.Register
{
    /// <summary>
    /// Implementação fake do repositório de pacientes para uso em testes de aplicação.
    /// Armazena os dados em memória, sem acesso a banco de dados.
    /// </summary>
    public class FakePatientRepository : IPatientRepository
    {
        private readonly List<SGHSS.Domain.Models.Patient> _patients = new();

        /// <summary>
        /// Exposição somente leitura da coleção interna de pacientes,
        /// útil para asserts adicionais nos testes.
        /// </summary>
        public IReadOnlyCollection<SGHSS.Domain.Models.Patient> Patients => _patients.AsReadOnly();

        public FakePatientRepository()
        {
            // ------------------------------
            //  Paciente 1
            // ------------------------------
            var p1 = new SGHSS.Domain.Models.Patient
            {
                Id = Guid.NewGuid(),
                Name = "Maria Oliveira",
                Email = new Email("maria@example.com"),
                Phone = new Phone("11999990001"),
                Cpf = new Cpf("334.408.471-28"),
                BirthDate = new DateTimeOffset(1990, 5, 20, 0, 0, 0, TimeSpan.Zero),
                Sex = Sex.Female,
                Address = new Address(
                    street: "Rua das Flores",
                    number: "100",
                    city: "São Paulo",
                    state: "SP",
                    cep: "01001000",
                    district: "Centro",
                    complement: null
                ),
                EmergencyContactName = "João Oliveira"
            };

            // ------------------------------
            //  Paciente 2
            // ------------------------------
            var p2 = new SGHSS.Domain.Models.Patient
            {
                Id = Guid.NewGuid(),
                Name = "João Silva",
                Email = new Email("joao@example.com"),
                Phone = new Phone("11888880002"),
                Cpf = new Cpf("334.882.138-03"),
                BirthDate = new DateTimeOffset(1985, 3, 15, 0, 0, 0, TimeSpan.Zero),
                Sex = Sex.Male,
                Address = new Address(
                    street: "Av Paulista",
                    number: "500",
                    city: "São Paulo",
                    state: "SP",
                    cep: "01311000",
                    district: "Bela Vista",
                    complement: "Apto 10"
                ),
                EmergencyContactName = "Mariana Silva"
            };

            // ------------------------------
            //  Paciente 3
            // ------------------------------
            var p3 = new SGHSS.Domain.Models.Patient
            {
                Id = Guid.NewGuid(),
                Name = "Carla Souza",
                Email = new Email("carla@example.com"),
                Phone = new Phone("11777770003"),
                Cpf = new Cpf("41019075953"),
                BirthDate = new DateTimeOffset(1992, 9, 10, 0, 0, 0, TimeSpan.Zero),
                Sex = Sex.Female,
                Address = new Address(
                    street: "Rua Azul",
                    number: "S/N",
                    city: "Rio de Janeiro",
                    state: "RJ",
                    cep: "20040002",
                    district: "Centro",
                    complement: null
                ),
                EmergencyContactName = "Paulo Souza"
            };

            _patients.AddRange(new[] { p1, p2, p3 });
        }

        public Task<bool> ExistsByCpfAsync(Cpf cpf)
        {
            bool exists = _patients.Any(p => p.Cpf.Value == cpf.Value);
            return Task.FromResult(exists);
        }

        public Task<bool> ExistsByEmailAsync(Email email)
        {
            bool exists = _patients.Any(p => p.Email.Value == email.Value);
            return Task.FromResult(exists);
        }

        public Task AddAsync(SGHSS.Domain.Models.Patient patient)
        {
            patient.Id = Guid.NewGuid();
            _patients.Add(patient);
            return Task.CompletedTask;
        }
    }
}
