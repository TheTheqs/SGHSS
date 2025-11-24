// Tests/Application/Administrator/Update/MakeBedAsAvailableUseCaseTests.cs

using FluentAssertions;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Administrators.Update;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using Xunit;

namespace SGHSS.Tests.Application.Administrator.Update
{
    public class MakeBedAsAvailableUseCaseTests
    {
        [Fact]
        public async Task Deve_Definir_Cama_Como_Disponivel_Quando_Estiver_Em_Manutencao()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var bedRepository = new BedRepository(context);
            var useCase = new MakeBedAsAvailableUseCase(bedRepository);

            var bed = new Bed
            {
                Id = Guid.NewGuid(),
                BedNumber = "BED-001",
                Type = BedType.General,
                Status = BedStatus.UnderMaintenance
            };

            context.Beds.Add(bed);
            await context.SaveChangesAsync();

            // Act
            await useCase.Handle(bed.Id);

            // Assert
            var updated = await bedRepository.GetByIdAsync(bed.Id);
            updated.Should().NotBeNull();
            updated!.Status.Should().Be(BedStatus.Available);
        }

        [Fact]
        public async Task Deve_Lancar_KeyNotFound_Quando_Cama_Nao_Existir()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var bedRepository = new BedRepository(context);
            var useCase = new MakeBedAsAvailableUseCase(bedRepository);

            Guid nonExistingBedId = Guid.NewGuid();

            // Act
            Func<Task> act = () => useCase.Handle(nonExistingBedId);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Nenhuma cama foi encontrada com o ID informado.");
        }

        [Fact]
        public async Task Deve_Recusar_Quando_Cama_Nao_Estiver_Em_Manutencao()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var bedRepository = new BedRepository(context);
            var useCase = new MakeBedAsAvailableUseCase(bedRepository);

            var bed = new Bed
            {
                Id = Guid.NewGuid(),
                BedNumber = "BED-002",
                Type = BedType.General,
                Status = BedStatus.Available // já está disponível
            };

            context.Beds.Add(bed);
            await context.SaveChangesAsync();

            // Act
            Func<Task> act = () => useCase.Handle(bed.Id);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("A cama só pode ser marcada como disponível se estiver em manutenção.");

            var persisted = await bedRepository.GetByIdAsync(bed.Id);
            persisted.Should().NotBeNull();
            persisted!.Status.Should().Be(BedStatus.Available);
        }
    }
}
