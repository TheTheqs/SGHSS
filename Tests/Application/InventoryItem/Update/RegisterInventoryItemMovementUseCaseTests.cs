// Tests/Application/InventoryItem/Update/RegisterInventoryItemMovementUseCaseTests.cs

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.InventoryItems.Register;
using SGHSS.Application.UseCases.InventoryItems.Update;
using SGHSS.Domain.Enums;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Models;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Administrators;
using SGHSS.Tests.TestData.Models.SGHSS.Tests.TestData.Common;
using Xunit;

namespace SGHSS.Tests.Application.InventoryItems.Update
{
    public class RegisterInventoryMovementUseCaseTests
    {
        [Fact]
        public async Task Deve_Registrar_Entrada_Quando_Dados_Forem_Validos()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IInventoryItemRepository inventoryItemRepo = new InventoryItemRepository(context);
            IAdministratorRepository administratorRepo = new AdministratorRepository(context);

            var registerHealthUnitUseCase = new RegisterHealthUnitUseCase(healthUnitRepo);
            var registerItemUseCase = new RegisterInventoryItemUseCase(inventoryItemRepo, healthUnitRepo);
            var registerAdministratorUseCase = new RegisterAdministratorUseCase(administratorRepo);

            var movementUseCase = new RegisterInventoryMovementUseCase(
                inventoryItemRepo,
                administratorRepo
            );

            // Unidade de saúde
            RegisterHealthUnitRequest healthUnitExample = HealthUnitGenerator.GenerateHealthUnit();
            var healthUnitResult = await registerHealthUnitUseCase.Handle(healthUnitExample);

            // Administrador responsável
            var administratorRequest = AdministratorGenerator.GenerateAdministrator();
            ConsentDto treatmentConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.App, true);
            ConsentDto notificationConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.App, true);

            administratorRequest.Consents.Add(treatmentConsent);
            administratorRequest.Consents.Add(notificationConsent);
            var administratorResult = await registerAdministratorUseCase.Handle(administratorRequest);

            // Item com estoque inicial conhecido
            const int initialStock = 10;
            var itemRequest = RegisterInventoryItemRequestGenerator.Generate(
                providedHealthUnitId: healthUnitResult.HealthUnitId,
                providedStockQuantity: initialStock
            );

            var itemResponse = await registerItemUseCase.Handle(itemRequest);

            const int movementQuantity = 5;

            var request = new RegisterInventoryMovementRequest
            {
                InventoryItemId = itemResponse.Id,
                HealthUnitId = healthUnitResult.HealthUnitId,
                AdministratorId = administratorResult.AdministratorId,
                Quantity = movementQuantity,
                MovementType = InventoryMovementType.Entry,
                Description = "Reposição de estoque."
            };

            var before = DateTimeOffset.UtcNow;

            // Act
            var response = await movementUseCase.Handle(request);

            var after = DateTimeOffset.UtcNow;

            // Assert – entidade
            var inventoryItem = await context.InventoryItems
                .Include(i => i.InventoryMovement)
                .Include(i => i.HealthUnit)
                .FirstAsync(i => i.Id == itemResponse.Id);

            inventoryItem.StockQuantity.Should().Be(initialStock + movementQuantity);
            inventoryItem.HealthUnit.Id.Should().Be(healthUnitResult.HealthUnitId);

            // Deve haver um movimento registrado
            var movement = inventoryItem.InventoryMovement.Should().ContainSingle().Subject;

            movement.Quantity.Should().Be(movementQuantity);
            movement.MovementType.Should().Be(InventoryMovementType.Entry);
            movement.Description.Should().Be(request.Description);
            movement.InventoryItem.Id.Should().Be(inventoryItem.Id);
            movement.Administrator.Id.Should().Be(administratorResult.AdministratorId);

            movement.MovementDate.Should().BeOnOrAfter(before);
            movement.MovementDate.Should().BeOnOrBefore(after);

            // Assert – response
            response.InventoryItemId.Should().Be(inventoryItem.Id);
            response.HealthUnitId.Should().Be(healthUnitResult.HealthUnitId);
            response.StockQuantity.Should().Be(inventoryItem.StockQuantity);
            response.Name.Should().Be(inventoryItem.Name);
            response.Description.Should().Be(inventoryItem.Description);
            response.UnitOfMeasure.Should().Be(inventoryItem.UnitOfMeasure.Value);
        }

        [Fact]
        public async Task Deve_Registrar_Saida_Quando_Estoque_For_Suficiente()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IInventoryItemRepository inventoryItemRepo = new InventoryItemRepository(context);
            IAdministratorRepository administratorRepo = new AdministratorRepository(context);

            var registerHealthUnitUseCase = new RegisterHealthUnitUseCase(healthUnitRepo);
            var registerItemUseCase = new RegisterInventoryItemUseCase(inventoryItemRepo, healthUnitRepo);
            var registerAdministratorUseCase = new RegisterAdministratorUseCase(administratorRepo);

            var movementUseCase = new RegisterInventoryMovementUseCase(
                inventoryItemRepo,
                administratorRepo
            );

            // Unidade de saúde
            RegisterHealthUnitRequest healthUnitExample = HealthUnitGenerator.GenerateHealthUnit();
            var healthUnitResult = await registerHealthUnitUseCase.Handle(healthUnitExample);

            // Administrador
            var administratorRequest = AdministratorGenerator.GenerateAdministrator();
            ConsentDto treatmentConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.App, true);
            ConsentDto notificationConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.App, true);

            administratorRequest.Consents.Add(treatmentConsent);
            administratorRequest.Consents.Add(notificationConsent);
            var administratorResult = await registerAdministratorUseCase.Handle(administratorRequest);

            const int initialStock = 20;
            const int movementQuantity = 7;

            var itemRequest = RegisterInventoryItemRequestGenerator.Generate(
                providedHealthUnitId: healthUnitResult.HealthUnitId,
                providedStockQuantity: initialStock
            );

            var itemResponse = await registerItemUseCase.Handle(itemRequest);

            var request = new RegisterInventoryMovementRequest
            {
                InventoryItemId = itemResponse.Id,
                HealthUnitId = healthUnitResult.HealthUnitId,
                AdministratorId = administratorResult.AdministratorId,
                Quantity = movementQuantity,
                MovementType = InventoryMovementType.Exit,
                Description = "Consumo em procedimentos."
            };

            // Act
            var response = await movementUseCase.Handle(request);

            // Assert
            var inventoryItem = await context.InventoryItems
                .Include(i => i.InventoryMovement)
                .FirstAsync(i => i.Id == itemResponse.Id);

            inventoryItem.StockQuantity.Should().Be(initialStock - movementQuantity);

            var movement = inventoryItem.InventoryMovement.Should().ContainSingle().Subject;
            movement.MovementType.Should().Be(InventoryMovementType.Exit);
            movement.Quantity.Should().Be(movementQuantity);

            response.StockQuantity.Should().Be(initialStock - movementQuantity);
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Saida_Deixar_Estoque_Negativo()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IInventoryItemRepository inventoryItemRepo = new InventoryItemRepository(context);
            IAdministratorRepository administratorRepo = new AdministratorRepository(context);

            var registerHealthUnitUseCase = new RegisterHealthUnitUseCase(healthUnitRepo);
            var registerItemUseCase = new RegisterInventoryItemUseCase(inventoryItemRepo, healthUnitRepo);
            var registerAdministratorUseCase = new RegisterAdministratorUseCase(administratorRepo);

            var movementUseCase = new RegisterInventoryMovementUseCase(
                inventoryItemRepo,
                administratorRepo
            );

            // Unidade de saúde
            RegisterHealthUnitRequest healthUnitExample = HealthUnitGenerator.GenerateHealthUnit();
            var healthUnitResult = await registerHealthUnitUseCase.Handle(healthUnitExample);

            // Administrador
            var administratorRequest = AdministratorGenerator.GenerateAdministrator();
            ConsentDto treatmentConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Treatment, ConsentChannel.App, true);
            ConsentDto notificationConsent =
                ConsentGenerator.GenerateConsent(ConsentScope.Notification, ConsentChannel.App, true);

            administratorRequest.Consents.Add(treatmentConsent);
            administratorRequest.Consents.Add(notificationConsent);

            var administratorResult = await registerAdministratorUseCase.Handle(administratorRequest);

            const int initialStock = 3;
            const int movementQuantity = 5;

            var itemRequest = RegisterInventoryItemRequestGenerator.Generate(
                providedHealthUnitId: healthUnitResult.HealthUnitId,
                providedStockQuantity: initialStock
            );

            var itemResponse = await registerItemUseCase.Handle(itemRequest);

            var request = new RegisterInventoryMovementRequest
            {
                InventoryItemId = itemResponse.Id,
                HealthUnitId = healthUnitResult.HealthUnitId,
                AdministratorId = administratorResult.AdministratorId,
                Quantity = movementQuantity,
                MovementType = InventoryMovementType.Exit,
                Description = "Tentativa de saída maior que o estoque."
            };

            // Act
            Func<Task> act = () => movementUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("A operação resultaria em estoque negativo, o que não é permitido.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Item_Nao_For_Encontrado()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IInventoryItemRepository inventoryItemRepo = new InventoryItemRepository(context);
            IAdministratorRepository administratorRepo = new AdministratorRepository(context);

            var movementUseCase = new RegisterInventoryMovementUseCase(
                inventoryItemRepo,
                administratorRepo
            );

            var request = new RegisterInventoryMovementRequest
            {
                InventoryItemId = Guid.NewGuid(),
                HealthUnitId = Guid.NewGuid(),
                AdministratorId = Guid.NewGuid(),
                Quantity = 5,
                MovementType = InventoryMovementType.Entry,
                Description = "Item inexistente."
            };

            // Act
            Func<Task> act = () => movementUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Item de estoque não encontrado para os identificadores informados.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Administrador_Nao_For_Encontrado()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IHealthUnitRepository healthUnitRepo = new HealthUnitRepository(context);
            IInventoryItemRepository inventoryItemRepo = new InventoryItemRepository(context);
            IAdministratorRepository administratorRepo = new AdministratorRepository(context);

            var registerHealthUnitUseCase = new RegisterHealthUnitUseCase(healthUnitRepo);
            var registerItemUseCase = new RegisterInventoryItemUseCase(inventoryItemRepo, healthUnitRepo);

            var movementUseCase = new RegisterInventoryMovementUseCase(
                inventoryItemRepo,
                administratorRepo
            );

            // Unidade de saúde
            RegisterHealthUnitRequest healthUnitExample = HealthUnitGenerator.GenerateHealthUnit();
            var healthUnitResult = await registerHealthUnitUseCase.Handle(healthUnitExample);

            // Item
            var itemRequest = RegisterInventoryItemRequestGenerator.Generate(
                providedHealthUnitId: healthUnitResult.HealthUnitId,
                providedStockQuantity: 10
            );

            var itemResponse = await registerItemUseCase.Handle(itemRequest);

            var request = new RegisterInventoryMovementRequest
            {
                InventoryItemId = itemResponse.Id,
                HealthUnitId = healthUnitResult.HealthUnitId,
                AdministratorId = Guid.NewGuid(), // inexistente
                Quantity = 2,
                MovementType = InventoryMovementType.Entry,
                Description = "Admin inexistente."
            };

            // Act
            Func<Task> act = () => movementUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Administrador não encontrado para o identificador informado.");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Quantidade_For_Menor_Ou_Igual_A_Zero()
        {
            // Arrange
            using var context = DbContextTestFactory.CreateInMemoryContext();

            IInventoryItemRepository inventoryItemRepo = new InventoryItemRepository(context);
            IAdministratorRepository administratorRepo = new AdministratorRepository(context);

            var movementUseCase = new RegisterInventoryMovementUseCase(
                inventoryItemRepo,
                administratorRepo
            );

            var request = new RegisterInventoryMovementRequest
            {
                InventoryItemId = Guid.NewGuid(),
                HealthUnitId = Guid.NewGuid(),
                AdministratorId = Guid.NewGuid(),
                Quantity = 0,
                MovementType = InventoryMovementType.Entry,
                Description = "Quantidade inválida."
            };

            // Act
            Func<Task> act = () => movementUseCase.Handle(request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("A quantidade da movimentação deve ser maior que zero.");
        }
    }
}
