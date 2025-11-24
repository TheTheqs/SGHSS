// Tests/Application/Administrator/Read/ConsultHealthUnitBedsUseCaseTests.cs

using FluentAssertions;
using SGHSS.Application.UseCases.Administrators.Read;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Models;
using Xunit;

namespace SGHSS.Tests.Application.Administrator.Read
{
    public class ConsultHealthUnitBedsUseCaseTests
    {
        /// <summary>
        /// Cria e persiste uma unidade de saúde de teste usando o RegisterHealthUnitUseCase.
        /// Retorna o Id da unidade criada.
        /// </summary>
        private static async Task<Guid> CreateHealthUnitAsync(HealthUnitRepository repo)
        {
            var request = HealthUnitGenerator.GenerateHealthUnit();
            var useCase = new RegisterHealthUnitUseCase(repo);
            var result = await useCase.Handle(request);
            return result.HealthUnitId;
        }

        [Fact]
        public async Task Deve_Retornar_Todas_As_Camas_Quando_Sem_Filtros()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var healthUnitRepository = new HealthUnitRepository(context);
            var useCase = new ConsultHealthUnitBedsUseCase(healthUnitRepository);

            var healthUnitId = await CreateHealthUnitAsync(healthUnitRepository);

            var healthUnit = await healthUnitRepository.GetByIdAsync(healthUnitId);
            healthUnit.Should().NotBeNull();

            healthUnit!.Beds.Add(new Bed
            {
                BedNumber = "BED-001",
                Type = BedType.General,
                Status = BedStatus.Available
            });

            healthUnit.Beds.Add(new Bed
            {
                BedNumber = "BED-002",
                Type = BedType.ICU,
                Status = BedStatus.Occupied
            });

            healthUnit.Beds.Add(new Bed
            {
                BedNumber = "BED-003",
                Type = BedType.Maternity,
                Status = BedStatus.UnderMaintenance
            });

            await healthUnitRepository.UpdateAsync(healthUnit);

            // Act
            var result = await useCase.Handle(healthUnitId);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(3);
        }

        [Fact]
        public async Task Deve_Filtrar_Por_Tipo_Quando_Tipo_For_Informado()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var healthUnitRepository = new HealthUnitRepository(context);
            var useCase = new ConsultHealthUnitBedsUseCase(healthUnitRepository);

            var healthUnitId = await CreateHealthUnitAsync(healthUnitRepository);

            var healthUnit = await healthUnitRepository.GetByIdAsync(healthUnitId);
            healthUnit.Should().NotBeNull();

            // 2 camas ICU e 1 General
            healthUnit!.Beds.Add(new Bed
            {
                BedNumber = "BED-010",
                Type = BedType.ICU,
                Status = BedStatus.Available
            });

            healthUnit.Beds.Add(new Bed
            {
                BedNumber = "BED-011",
                Type = BedType.ICU,
                Status = BedStatus.Occupied
            });

            healthUnit.Beds.Add(new Bed
            {
                BedNumber = "BED-012",
                Type = BedType.General,
                Status = BedStatus.Available
            });

            await healthUnitRepository.UpdateAsync(healthUnit);

            // Act
            var result = await useCase.Handle(healthUnitId, type: BedType.ICU);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
            result.All(b => b.Type == BedType.ICU).Should().BeTrue();
        }

        [Fact]
        public async Task Deve_Filtrar_Por_Status_Quando_Status_For_Informado()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var healthUnitRepository = new HealthUnitRepository(context);
            var useCase = new ConsultHealthUnitBedsUseCase(healthUnitRepository);

            var healthUnitId = await CreateHealthUnitAsync(healthUnitRepository);

            var healthUnit = await healthUnitRepository.GetByIdAsync(healthUnitId);
            healthUnit.Should().NotBeNull();

            // 2 Occupied, 1 Available
            healthUnit!.Beds.Add(new Bed
            {
                BedNumber = "BED-020",
                Type = BedType.General,
                Status = BedStatus.Occupied
            });

            healthUnit.Beds.Add(new Bed
            {
                BedNumber = "BED-021",
                Type = BedType.ICU,
                Status = BedStatus.Occupied
            });

            healthUnit.Beds.Add(new Bed
            {
                BedNumber = "BED-022",
                Type = BedType.Maternity,
                Status = BedStatus.Available
            });

            await healthUnitRepository.UpdateAsync(healthUnit);

            // Act
            var result = await useCase.Handle(healthUnitId, status: BedStatus.Occupied);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
            result.All(b => b.Status == BedStatus.Occupied).Should().BeTrue();
        }

        [Fact]
        public async Task Deve_Filtrar_Por_Tipo_E_Status_Quando_Ambos_Forem_Informados()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var healthUnitRepository = new HealthUnitRepository(context);
            var useCase = new ConsultHealthUnitBedsUseCase(healthUnitRepository);

            var healthUnitId = await CreateHealthUnitAsync(healthUnitRepository);

            var healthUnit = await healthUnitRepository.GetByIdAsync(healthUnitId);
            healthUnit.Should().NotBeNull();

            // 2 ICU (1 Available, 1 Occupied) + 1 General Available
            healthUnit!.Beds.Add(new Bed
            {
                BedNumber = "BED-030",
                Type = BedType.ICU,
                Status = BedStatus.Available
            });

            healthUnit.Beds.Add(new Bed
            {
                BedNumber = "BED-031",
                Type = BedType.ICU,
                Status = BedStatus.Occupied
            });

            healthUnit.Beds.Add(new Bed
            {
                BedNumber = "BED-032",
                Type = BedType.General,
                Status = BedStatus.Available
            });

            await healthUnitRepository.UpdateAsync(healthUnit);

            // Act
            var result = await useCase.Handle(healthUnitId, type: BedType.ICU, status: BedStatus.Available);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result.Single().Type.Should().Be(BedType.ICU);
            result.Single().Status.Should().Be(BedStatus.Available);
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Unidade_Nao_Existir()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var healthUnitRepository = new HealthUnitRepository(context);
            var useCase = new ConsultHealthUnitBedsUseCase(healthUnitRepository);

            Guid nonExistingId = Guid.NewGuid();

            // Act
            Func<Task> act = () => useCase.Handle(nonExistingId);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("A unidade de saúde informada não existe.");
        }
    }
}
