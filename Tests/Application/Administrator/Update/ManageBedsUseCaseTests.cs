// Tests/Application/Administrator/Update/ManageBedsUseCaseTests.cs

using FluentAssertions;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.Administrators.Update;
using SGHSS.Application.UseCases.Common;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Models;
using Xunit;

namespace SGHSS.Tests.Application.Administrator.Update
{
    public class ManageBedsUseCaseTests
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
        public async Task Deve_Adicionar_Leitos_Com_Sucesso()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new HealthUnitRepository(context);

            var healthUnitId = await CreateHealthUnitAsync(repo);

            BedDto bedDto = BedGenerator.GenerateBed(type: BedType.General, status: BedStatus.Available);

            var request = new ManageBedsRequest
            {
                HealthUnitId = healthUnitId,
                TotalBeds = 0,
                Bed = bedDto,
                Quantity = 3,
                IsAdding = true
            };

            var useCase = new ManageBedsUseCase(repo);

            // Act
            var result = await useCase.Handle(request);

            // Assert
            result.Should().NotBeNull();
            result.HealthUnitId.Should().Be(healthUnitId);
            result.Beds.Count.Should().Be(3);
            result.Beds.All(b => b.Type == bedDto.Type && b.Status == bedDto.Status).Should().BeTrue();

            var persisted = await repo.GetByIdAsync(healthUnitId);
            persisted.Should().NotBeNull();
            persisted!.Beds.Count.Should().Be(3);
        }

        [Fact]
        public async Task Deve_Remover_Leitos_Com_Sucesso()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new HealthUnitRepository(context);

            var healthUnitId = await CreateHealthUnitAsync(repo);
            var useCase = new ManageBedsUseCase(repo);

            BedDto bedDto = BedGenerator.GenerateBed(type: BedType.General, status: BedStatus.Available);

            // Primeiro adiciona 5 leitos
            var addRequest = new ManageBedsRequest
            {
                HealthUnitId = healthUnitId,
                TotalBeds = 0,
                Bed = bedDto,
                Quantity = 5,
                IsAdding = true
            };

            var addResult = await useCase.Handle(addRequest);
            addResult.Beds.Count.Should().Be(5);

            // Agora remove 2 leitos com o mesmo tipo e status
            var removeRequest = new ManageBedsRequest
            {
                HealthUnitId = healthUnitId,
                TotalBeds = 5,
                Bed = new BedDto
                {
                    BedNumber = string.Empty, // não é usado no filtro por modelo
                    Type = bedDto.Type,
                    Status = bedDto.Status
                },
                Quantity = 2,
                IsAdding = false
            };

            // Act
            var removeResult = await useCase.Handle(removeRequest);

            // Assert
            removeResult.Should().NotBeNull();
            removeResult.HealthUnitId.Should().Be(healthUnitId);
            removeResult.Beds.Count.Should().Be(3);

            var persisted = await repo.GetByIdAsync(healthUnitId);
            persisted.Should().NotBeNull();
            persisted!.Beds.Count.Should().Be(3);
        }

        [Fact]
        public async Task Deve_Recusar_Quantidade_Inferior_A_Um()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new HealthUnitRepository(context);

            var healthUnitId = await CreateHealthUnitAsync(repo);

            BedDto bedDto = BedGenerator.GenerateBed(status: BedStatus.Available);

            var request = new ManageBedsRequest
            {
                HealthUnitId = healthUnitId,
                TotalBeds = 0,
                Bed = bedDto,
                Quantity = 0,
                IsAdding = true
            };

            var useCase = new ManageBedsUseCase(repo);

            // Act
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("A quantidade de leitos deve ser maior ou igual a 1.");
        }

        [Fact]
        public async Task Deve_Recusar_Operacao_Com_Leito_Ocupado()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new HealthUnitRepository(context);

            var healthUnitId = await CreateHealthUnitAsync(repo);

            BedDto occupiedBed = BedGenerator.GenerateBed(status: BedStatus.Occupied);

            var request = new ManageBedsRequest
            {
                HealthUnitId = healthUnitId,
                TotalBeds = 0,
                Bed = occupiedBed,
                Quantity = 1,
                IsAdding = true
            };

            var useCase = new ManageBedsUseCase(repo);

            // Act
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Leitos com status 'Occupied' não podem ser adicionados ou removidos manualmente.");
        }

        [Fact]
        public async Task Deve_Recusar_Operacao_Em_Unidade_Inexistente()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new HealthUnitRepository(context);

            BedDto bedDto = BedGenerator.GenerateBed(status: BedStatus.Available);

            var request = new ManageBedsRequest
            {
                HealthUnitId = Guid.NewGuid(), // não existe no contexto
                TotalBeds = 0,
                Bed = bedDto,
                Quantity = 1,
                IsAdding = true
            };

            var useCase = new ManageBedsUseCase(repo);

            // Act
            Func<Task> act = () => useCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Unidade de saúde não encontrada para o identificador informado.");
        }

        [Fact]
        public async Task Deve_Recusar_Remocao_Com_Leitos_Insuficientes()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new HealthUnitRepository(context);

            var healthUnitId = await CreateHealthUnitAsync(repo);
            var useCase = new ManageBedsUseCase(repo);

            BedDto bedDto = BedGenerator.GenerateBed(type: BedType.General, status: BedStatus.Available);

            // Adiciona apenas 2 leitos
            var addRequest = new ManageBedsRequest
            {
                HealthUnitId = healthUnitId,
                TotalBeds = 0,
                Bed = bedDto,
                Quantity = 2,
                IsAdding = true
            };

            await useCase.Handle(addRequest);

            // Tenta remover 3
            var removeRequest = new ManageBedsRequest
            {
                HealthUnitId = healthUnitId,
                TotalBeds = 2,
                Bed = new BedDto
                {
                    BedNumber = string.Empty,
                    Type = bedDto.Type,
                    Status = bedDto.Status
                },
                Quantity = 3,
                IsAdding = false
            };

            // Act
            Func<Task> act = () => useCase.Handle(removeRequest);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("A unidade de saúde não possui leitos suficientes para serem removidos com os critérios informados.");

            var persisted = await repo.GetByIdAsync(healthUnitId);
            persisted.Should().NotBeNull();
            persisted!.Beds.Count.Should().Be(2);
        }

        [Fact]
        public async Task Deve_Remover_Leito_Por_Numero_Com_Sucesso()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new HealthUnitRepository(context);

            var healthUnitId = await CreateHealthUnitAsync(repo);
            var useCase = new ManageBedsUseCase(repo);

            BedDto bedDto = BedGenerator.GenerateBed(status: BedStatus.Available);

            // Adiciona 2 leitos iguais em tipo/status
            var addRequest = new ManageBedsRequest
            {
                HealthUnitId = healthUnitId,
                TotalBeds = 0,
                Bed = bedDto,
                Quantity = 2,
                IsAdding = true
            };

            var addResult = await useCase.Handle(addRequest);
            addResult.Beds.Count.Should().Be(2);

            var persisted = await repo.GetByIdAsync(healthUnitId);
            persisted.Should().NotBeNull();
            var targetBedNumber = persisted!.Beds.First().BedNumber;

            // Act
            var result = await useCase.RemoveBedByNumberAsync(healthUnitId, targetBedNumber);

            // Assert
            result.Should().NotBeNull();
            result.Beds.Count.Should().Be(1);

            var updated = await repo.GetByIdAsync(healthUnitId);
            updated.Should().NotBeNull();
            updated!.Beds.Count.Should().Be(1);
        }

        [Fact]
        public async Task Deve_Recusar_RemoveBedByNumber_Em_Unidade_Inexistente()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new HealthUnitRepository(context);
            var useCase = new ManageBedsUseCase(repo);

            // Act
            Func<Task> act = () => useCase.RemoveBedByNumberAsync(Guid.NewGuid(), "BED-001");

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Unidade de saúde não encontrada para o identificador informado.");
        }

        [Fact]
        public async Task Deve_Recusar_RemoveBedByNumber_Quando_Leito_Nao_Existe()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new HealthUnitRepository(context);

            var healthUnitId = await CreateHealthUnitAsync(repo);
            var useCase = new ManageBedsUseCase(repo);

            // Act
            Func<Task> act = () => useCase.RemoveBedByNumberAsync(healthUnitId, "BED-999");

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("A unidade de saúde não possui um leito com o número informado.");
        }

        [Fact]
        public async Task Deve_Recusar_RemoveBedByNumber_Para_Leito_Ocupado()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new HealthUnitRepository(context);

            var healthUnitId = await CreateHealthUnitAsync(repo);

            // Cria manualmente um leito ocupado na unidade, simulando um cenário de internação
            var healthUnit = await repo.GetByIdAsync(healthUnitId);
            healthUnit.Should().NotBeNull();

            var occupiedBed = new Bed
            {
                BedNumber = "BED-001",
                Type = BedType.General,
                Status = BedStatus.Occupied
            };

            healthUnit!.Beds.Add(occupiedBed);
            await repo.UpdateAsync(healthUnit);

            var useCase = new ManageBedsUseCase(repo);

            // Act
            Func<Task> act = () => useCase.RemoveBedByNumberAsync(healthUnitId, "BED-001");

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Não é possível remover um leito que está com status 'Occupied'.");
        }
    }
}
