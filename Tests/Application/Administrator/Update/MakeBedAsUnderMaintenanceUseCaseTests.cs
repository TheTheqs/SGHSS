// Tests/Application/Administrator/Update/MakeBedAsUnderMaintenanceUseCaseTests.cs

using FluentAssertions;
using SGHSS.Application.UseCases.Administrators.Update;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using Xunit;

namespace SGHSS.Tests.Application.Administrator.Update
{
    public class MakeBedAsUnderMaintenanceUseCaseTests
    {
        [Fact]
        public async Task Deve_Definir_Cama_Como_Em_Manutencao_Quando_Estiver_Disponivel()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var bedRepository = new BedRepository(context);
            var useCase = new MakeBedAsUnderMaintenanceUseCase(bedRepository);

            var bed = new Bed
            {
                Id = Guid.NewGuid(),
                BedNumber = "BED-010",
                Type = BedType.General,
                Status = BedStatus.Available
            };

            context.Beds.Add(bed);
            await context.SaveChangesAsync();

            // Act
            await useCase.Handle(bed.Id);

            // Assert
            var updated = await bedRepository.GetByIdAsync(bed.Id);
            updated.Should().NotBeNull();
            updated!.Status.Should().Be(BedStatus.UnderMaintenance);
        }

        [Fact]
        public async Task Deve_Lancar_KeyNotFound_Quando_Cama_Nao_Existir()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var bedRepository = new BedRepository(context);
            var useCase = new MakeBedAsUnderMaintenanceUseCase(bedRepository);

            Guid nonExistingId = Guid.NewGuid();

            // Act
            Func<Task> act = () => useCase.Handle(nonExistingId);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Nenhuma cama foi encontrada com o ID informado.");
        }

        [Theory]
        [InlineData(BedStatus.Occupied)]
        [InlineData(BedStatus.UnderMaintenance)]
        public async Task Deve_Recusar_Alteracao_Quando_Cama_Nao_Estiver_Disponivel(BedStatus initialStatus)
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var bedRepository = new BedRepository(context);
            var useCase = new MakeBedAsUnderMaintenanceUseCase(bedRepository);

            var bed = new Bed
            {
                Id = Guid.NewGuid(),
                BedNumber = "BED-777",
                Type = BedType.General,
                Status = initialStatus
            };

            context.Beds.Add(bed);
            await context.SaveChangesAsync();

            // Act
            Func<Task> act = () => useCase.Handle(bed.Id);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("A cama só pode ser colocada em manutenção se estiver disponível.");

            var persisted = await bedRepository.GetByIdAsync(bed.Id);
            persisted.Should().NotBeNull();
            persisted!.Status.Should().Be(initialStatus); // não muda
        }
    }
}
