// Tests/Application/InventoryItem/Register/RegisterInventoryItemUseCaseTests.cs

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.InventoryItems.Register;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Models;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Common;
using Xunit;

namespace SGHSS.Tests.Application.InventoryItems.Register
{
    public class RegisterInventoryItemUseCaseTests
    {
        [Fact]
        public async Task Deve_Registrar_Item_De_Estoque_Quando_Dados_Forem_Validos()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IInventoryItemRepository inventoryItemRepo = new InventoryItemRepository(context);

            var registerHealthUnitUseCase = new RegisterHealthUnitUseCase(healthUnitRepo);

            // Unidade de saúde válida
            RegisterHealthUnitRequest healthUnitExample = HealthUnitGenerator.GenerateHealthUnit();
            var healthUnitResult = await registerHealthUnitUseCase.Handle(healthUnitExample);

            // Carrega a unidade rastreada
            var healthUnit = await context.HealthUnits
                .FirstAsync(h => h.Id == healthUnitResult.HealthUnitId);

            var registerItemUseCase = new RegisterInventoryItemUseCase(
                inventoryItemRepo,
                healthUnitRepo
            );

            var request = RegisterInventoryItemRequestGenerator.Generate(
                providedHealthUnitId: healthUnitResult.HealthUnitId
            );

            // Act
            var response = await registerItemUseCase.Handle(request);

            // Assert – entidade
            var inventoryItem = await context.InventoryItems
                .Include(i => i.HealthUnit)
                .FirstAsync(i => i.Id == response.Id);

            inventoryItem.Name.Should().Be(request.Name.Trim());
            inventoryItem.Description.Should().Be(request.Description.Trim());
            inventoryItem.StockQuantity.Should().Be(request.StockQuantity);

            inventoryItem.UnitOfMeasure.Value.Should().Be(response.UnitOfMeasure);
            inventoryItem.HealthUnit.Id.Should().Be(healthUnit.Id);

            // Assert – response
            response.Should().NotBeNull();
            response.Id.Should().Be(inventoryItem.Id);
            response.Name.Should().Be(inventoryItem.Name);
            response.Description.Should().Be(inventoryItem.Description);
            response.StockQuantity.Should().Be(inventoryItem.StockQuantity);
            response.UnitOfMeasure.Should().Be(inventoryItem.UnitOfMeasure.Value);
            response.HealthUnitId.Should().Be(healthUnit.Id);
        }

        [Fact]
        public async Task Deve_Normalizar_Unidade_De_Medida_Quando_Alias_For_Utilizado()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IInventoryItemRepository inventoryItemRepo = new InventoryItemRepository(context);

            var registerHealthUnitUseCase = new RegisterHealthUnitUseCase(healthUnitRepo);

            // Unidade de saúde
            RegisterHealthUnitRequest healthUnitExample = HealthUnitGenerator.GenerateHealthUnit();
            var healthUnitResult = await registerHealthUnitUseCase.Handle(healthUnitExample);

            var registerItemUseCase = new RegisterInventoryItemUseCase(
                inventoryItemRepo,
                healthUnitRepo
            );

            // Usa um alias conhecido do VO (ex.: "quilograma" -> "KG")
            var request = RegisterInventoryItemRequestGenerator.Generate(
                providedHealthUnitId: healthUnitResult.HealthUnitId,
                providedUnitOfMeasure: "quilograma"
            );

            // Act
            var response = await registerItemUseCase.Handle(request);

            // Assert
            var inventoryItem = await context.InventoryItems
                .FirstAsync(i => i.Id == response.Id);

            inventoryItem.UnitOfMeasure.Value.Should().Be("KG");
            response.UnitOfMeasure.Should().Be("KG");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Unidade_De_Saude_Nao_For_Encontrada()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IInventoryItemRepository inventoryItemRepo = new InventoryItemRepository(context);

            var registerItemUseCase = new RegisterInventoryItemUseCase(
                inventoryItemRepo,
                healthUnitRepo
            );

            // Usa um HealthUnitId inexistente
            var request = RegisterInventoryItemRequestGenerator.Generate(
                providedHealthUnitId: Guid.NewGuid()
            );

            // Act
            Func<Task> act = () => registerItemUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Unidade de saúde informada não foi encontrada.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Nome_Do_Item_Estiver_Vazio()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IInventoryItemRepository inventoryItemRepo = new InventoryItemRepository(context);

            var registerItemUseCase = new RegisterInventoryItemUseCase(
                inventoryItemRepo,
                healthUnitRepo
            );

            var request = RegisterInventoryItemRequestGenerator.Generate(
                providedHealthUnitId: Guid.NewGuid()
            );

            request.Name = " "; // força nome vazio/whitespace

            // Act
            Func<Task> act = () => registerItemUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("É obrigatório informar o nome do item de estoque.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Unidade_De_Medida_Estiver_Vazia()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IInventoryItemRepository inventoryItemRepo = new InventoryItemRepository(context);

            var registerItemUseCase = new RegisterInventoryItemUseCase(
                inventoryItemRepo,
                healthUnitRepo
            );

            var request = RegisterInventoryItemRequestGenerator.Generate(
                providedHealthUnitId: Guid.NewGuid()
            );

            request.UnitOfMeasure = " "; // força vazio/whitespace

            // Act
            Func<Task> act = () => registerItemUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("É obrigatório informar a unidade de medida do item de estoque.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Quantidade_Estiver_Negativa()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IInventoryItemRepository inventoryItemRepo = new InventoryItemRepository(context);

            var registerItemUseCase = new RegisterInventoryItemUseCase(
                inventoryItemRepo,
                healthUnitRepo
            );

            var request = RegisterInventoryItemRequestGenerator.Generate(
                providedHealthUnitId: Guid.NewGuid(),
                providedStockQuantity: -5
            );

            // Act
            Func<Task> act = () => registerItemUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("A quantidade em estoque não pode ser negativa.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_HealthUnitId_For_Vazio()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IInventoryItemRepository inventoryItemRepo = new InventoryItemRepository(context);

            var registerItemUseCase = new RegisterInventoryItemUseCase(
                inventoryItemRepo,
                healthUnitRepo
            );

            var request = RegisterInventoryItemRequestGenerator.Generate(
                providedHealthUnitId: Guid.Empty
            );

            // Act
            Func<Task> act = () => registerItemUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("É obrigatório informar uma unidade de saúde válida para o item de estoque.");
        }
    }
}
