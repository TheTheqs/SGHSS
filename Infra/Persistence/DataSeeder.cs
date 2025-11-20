// Infra/Persistence/DataSeeder.cs


using Microsoft.EntityFrameworkCore;
using SGHSS.Domain.Models;
using SGHSS.Domain.Enums;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Infra.Persistence
{
    /// <summary>
    /// Popula o banco de dados com dados iniciais para testes.
    /// </summary>
    public static class DataSeeder
    {
        public static async Task SeedAsync(SGHSSDbContext context)
        {
            await context.Database.MigrateAsync();

            // Verifica se já existe algum Patient
            if (!context.Patients.Any())
            {
                // ------------------------------
                //  Paciente 1
                // ------------------------------
                var p1 = new SGHSS.Domain.Models.Patient
                {
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

                context.Patients.Add(p1);
                context.Patients.Add(p2);
                context.Patients.Add(p3);
            }            

            await context.SaveChangesAsync();
        }
    }
}
