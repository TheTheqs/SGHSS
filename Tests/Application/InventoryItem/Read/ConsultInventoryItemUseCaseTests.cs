// Tests/Application/InventoryItem/Register/ConsultInventoryItemUseCaseTests.cs

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.InventoryItems.Consult;
using SGHSS.Application.UseCases.InventoryItems.Register;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Models;
using Xunit;

namespace SGHSS.Tests.Application.InventoryItems.Consult
{
    public class ConsultInventoryItemUseCaseTests
    {
        [Fact]
        public async Task Deve_Consultar_Item_De_Estoque_Quando_Dados_Forem_Validos()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IInventoryItemRepository inventoryItemRepo = new InventoryItemRepository(context);

            var registerHealthUnitUseCase = new RegisterHealthUnitUseCase(healthUnitRepo);
            var registerItemUseCase = new RegisterInventoryItemUseCase(inventoryItemRepo, healthUnitRepo);
            var consultItemUseCase = new ConsultInventoryItemUseCase(inventoryItemRepo);

            // Unidade de saúde válida
            RegisterHealthUnitRequest healthUnitExample = HealthUnitGenerator.GenerateHealthUnit();
            var healthUnitResult = await registerHealthUnitUseCase.Handle(healthUnitExample);

            // Item de estoque vinculado à unidade
            var registerRequest = RegisterInventoryItemRequestGenerator.Generate(
                providedHealthUnitId: healthUnitResult.HealthUnitId
            );

            var registerResponse = await registerItemUseCase.Handle(registerRequest);

            // Request de consulta
            var consultRequest = new ConsultInventoryItemRequest
            {
                InventoryItemId = registerResponse.Id,
                HealthUnitId = healthUnitResult.HealthUnitId
            };

            // Act
            var response = await consultItemUseCase.Handle(consultRequest);

            // Assert – entidade
            var inventoryItem = await context.InventoryItems
                .Include(i => i.HealthUnit)
                .FirstAsync(i => i.Id == registerResponse.Id);

            inventoryItem.Id.Should().Be(response.InventoryItemId);
            inventoryItem.Name.Should().Be(response.Name);
            inventoryItem.Description.Should().Be(response.Description);
            inventoryItem.StockQuantity.Should().Be(response.StockQuantity);
            inventoryItem.UnitOfMeasure.Value.Should().Be(response.UnitOfMeasure);
            inventoryItem.HealthUnit.Id.Should().Be(response.HealthUnitId);

            // Assert – response vs request
            response.InventoryItemId.Should().Be(registerResponse.Id);
            response.HealthUnitId.Should().Be(healthUnitResult.HealthUnitId);
            response.Name.Should().Be(registerResponse.Name);
            response.Description.Should().Be(registerResponse.Description);
            response.UnitOfMeasure.Should().Be(registerResponse.UnitOfMeasure);
            response.StockQuantity.Should().Be(registerResponse.StockQuantity);
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Item_Nao_For_Encontrado()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IInventoryItemRepository inventoryItemRepo = new InventoryItemRepository(context);
            var consultItemUseCase = new ConsultInventoryItemUseCase(inventoryItemRepo);

            var request = new ConsultInventoryItemRequest
            {
                InventoryItemId = Guid.NewGuid(),     // inexistente
                HealthUnitId = Guid.NewGuid()
            };

            // Act
            Func<Task> act = () => consultItemUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Item de estoque não encontrado para os identificadores informados.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Item_Nao_Pertencer_A_Unidade_De_Saude()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IInventoryItemRepository inventoryItemRepo = new InventoryItemRepository(context);

            var registerHealthUnitUseCase = new RegisterHealthUnitUseCase(healthUnitRepo);
            var registerItemUseCase = new RegisterInventoryItemUseCase(inventoryItemRepo, healthUnitRepo);
            var consultItemUseCase = new ConsultInventoryItemUseCase(inventoryItemRepo);

            // Unidade A
            RegisterHealthUnitRequest healthUnitAExample = HealthUnitGenerator.GenerateHealthUnit();
            var healthUnitAResult = await registerHealthUnitUseCase.Handle(healthUnitAExample);

            // Unidade B
            RegisterHealthUnitRequest healthUnitBExample = HealthUnitGenerator.GenerateHealthUnit();
            var healthUnitBResult = await registerHealthUnitUseCase.Handle(healthUnitBExample);

            // Item vinculado à unidade A
            var registerRequest = RegisterInventoryItemRequestGenerator.Generate(
                providedHealthUnitId: healthUnitAResult.HealthUnitId
            );

            var registerResponse = await registerItemUseCase.Handle(registerRequest);

            // Tenta consultar o item pela unidade B (errado)
            var consultRequest = new ConsultInventoryItemRequest
            {
                InventoryItemId = registerResponse.Id,
                HealthUnitId = healthUnitBResult.HealthUnitId
            };

            // Act
            Func<Task> act = () => consultItemUseCase.Handle(consultRequest);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Item de estoque não encontrado para os identificadores informados.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_InventoryItemId_For_Vazio()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IInventoryItemRepository inventoryItemRepo = new InventoryItemRepository(context);
            var consultItemUseCase = new ConsultInventoryItemUseCase(inventoryItemRepo);

            var request = new ConsultInventoryItemRequest
            {
                InventoryItemId = Guid.Empty,
                HealthUnitId = Guid.NewGuid()
            };

            // Act
            Func<Task> act = () => consultItemUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("É obrigatório informar um identificador de item de estoque válido.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_HealthUnitId_For_Vazio()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IInventoryItemRepository inventoryItemRepo = new InventoryItemRepository(context);
            var consultItemUseCase = new ConsultInventoryItemUseCase(inventoryItemRepo);

            var request = new ConsultInventoryItemRequest
            {
                InventoryItemId = Guid.NewGuid(),
                HealthUnitId = Guid.Empty
            };

            // Act
            Func<Task> act = () => consultItemUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("É obrigatório informar um identificador de unidade de saúde válido.");
        }
    }
}
