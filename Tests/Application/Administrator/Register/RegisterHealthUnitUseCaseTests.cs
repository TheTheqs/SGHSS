// Tests/Application/Administrator/Register/RegisterHealthUnitTests.cs

using FluentAssertions;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Domain.ValueObjects;
using SGHSS.Infra.Repositories;
using SGHSS.Tests.Infra;
using SGHSS.Tests.TestData.Models;
using Xunit;

namespace SGHSS.Tests.Application.Administrator.Register
{
    public class RegisterHealthUnitTests
    {
        [Fact]
        public async Task Deve_Salvar_Nova_Unidade_de_Saude()
        {
            // Arrange
            RegisterHealthUnitRequest example = HealthUnitGenerator.GenerateHealthUnit();

            // In memory database
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new HealthUnitRepository(context);

            // Act
            var useCase = new RegisterHealthUnitUseCase(repo);
            var request = example;
            var result = await useCase.Handle(request);
            var consult = await repo.ExistsByCnpjAsync(new Cnpj(request.Cnpj));

            // Assert
            result.Should().NotBeNull();
            result.HealthUnitId.Should().NotBe(Guid.Empty);
            consult.Should().BeTrue();
        }

        [Fact]
        public async Task Deve_Recusar_cnpj_duplicado()
        {
            // Arrange
            RegisterHealthUnitRequest example = HealthUnitGenerator.GenerateHealthUnit();
            RegisterHealthUnitRequest example2 = HealthUnitGenerator.GenerateHealthUnit(providedCnpj: example.Cnpj);

            // In memory database
            using var context = DbContextTestFactory.CreateInMemoryContext();
            var repo = new HealthUnitRepository(context);

            // Act 1 - registra a primeira unidade
            var useCase = new RegisterHealthUnitUseCase(repo);
            var request = example;
            var result = await useCase.Handle(request);
            var consult = await repo.ExistsByCnpjAsync(new Cnpj(request.Cnpj));

            result.Should().NotBeNull();
            result.HealthUnitId.Should().NotBe(Guid.Empty);
            consult.Should().BeTrue();

            // Act 2 - tenta registrar outra com o mesmo CNPJ
            var request2 = example2;
            Func<Task> act = () => useCase.Handle(request2);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Já existe uma unidade de saúde cadastrada com o CNPJ informado.");
        }
    }
}
