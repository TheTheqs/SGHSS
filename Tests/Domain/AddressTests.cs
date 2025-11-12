// Tests/Domain/AddressTests.cs

using FluentAssertions;
using SGHSS.Domain.ValueObjects;
using System.Net;
using Xunit;

namespace SGHSS.Tests.Domain
{
    public class AddressTests
    {
        [Fact]
        public void Deve_instanciar_normalizar_e_montar_Value_canonico()
        {
            var addr = new Address(
                street: "  Av. Paulista  ",
                number: "  1578  ",
                city: " São Paulo ",
                state: " sp ",
                cep: " 01310-200 ",
                district: " Bela Vista ",
                complement: "  Apto 101 "
            );

            addr.Street.Should().Be("Av. Paulista");
            addr.Number.Should().Be("1578");
            addr.City.Should().Be("São Paulo");
            addr.State.Should().Be("SP");
            addr.Cep.Should().Be("01310200"); // só dígitos
            addr.District.Should().Be("Bela Vista");
            addr.Complement.Should().Be("Apto 101");

            addr.Value.Should().Be("Av. Paulista, 1578 - Apto 101 - Bela Vista, São Paulo - SP, 01310-200");
            addr.ToString().Should().Be(addr.Value);
        }

        [Fact]
        public void Deve_formatar_sem_bairro_ou_complemento()
        {
            var addr = new Address(
                street: "Rua X",
                number: "S/N",
                city: "Indaiatuba",
                state: "SP",
                cep: "13330-000"
            );

            addr.Value.Should().Be("Rua X, S/N, Indaiatuba - SP, 13330-000");
        }

        [Theory]
        [InlineData("", "123", "Cidade", "SP", "01310-200")]     // street vazio
        [InlineData("Rua", "", "Cidade", "SP", "01310-200")]     // number vazio
        [InlineData("Rua", "123", "", "SP", "01310-200")]        // city vazia
        [InlineData("Rua", "123", "Cidade", "XX", "01310-200")]  // UF inválida
        [InlineData("Rua", "123", "Cidade", "SP", "0131-020")]   // CEP mal formatado
        public void Deve_lancar_para_valores_invalidos(
    string street, string number, string city, string state, string cep)
        {
            Action act = () => new Address(street, number, city, state, cep);
            act.Should().Throw<ArgumentException>();
        }
    }
}
