// Tests/Domain/UnitOfMeasureTests.cs

using FluentAssertions;
using SGHSS.Domain.ValueObjects;
using Xunit;

namespace SGHSS.Tests.Domain
{
    public class UnitOfMeasureTests
    {
        [Fact]
        public void Deve_instanciar_normalizar_aliases_com_igualdade_por_valor()
        {
            // KG
            var a1 = new UnitOfMeasure("kg");
            var a2 = new UnitOfMeasure("Kg");
            var a3 = new UnitOfMeasure("KILO");
            var a4 = new UnitOfMeasure("quilograma");

            a1.Value.Should().Be("KG");
            a2.Value.Should().Be("KG");
            a3.Value.Should().Be("KG");
            a4.Value.Should().Be("KG");

            a1.Should().Be(a2);
            a1.Should().Be(a3);
            a1.Should().Be(a4);

            a1.ToString().Should().Be("KG");

            // UN (unidade/peça)
            var b1 = new UnitOfMeasure("un");
            var b2 = new UnitOfMeasure("UNIDADE");
            var b3 = new UnitOfMeasure("unit");
            var b4 = new UnitOfMeasure("UND");

            b1.Value.Should().Be("UN");
            b2.Value.Should().Be("UN");
            b3.Value.Should().Be("UN");
            b4.Value.Should().Be("UN");

            // ML
            var c1 = new UnitOfMeasure("ml");
            var c2 = new UnitOfMeasure("mililitro");
            c1.Value.Should().Be("ML");
            c2.Value.Should().Be("ML");

            // M2
            var d1 = new UnitOfMeasure("m2");
            var d2 = new UnitOfMeasure("M2");
            var d3 = new UnitOfMeasure("metroquadrado");
            d1.Value.Should().Be("M2");
            d3.Value.Should().Be("M2");

            // Código custom (mantém)
            var e1 = new UnitOfMeasure("Kit01");
            e1.Value.Should().Be("KIT01");
        }

        [Theory]
        [InlineData("   ")]          // vazio
        [InlineData("kg/m")]         // composto (não aceita operações/compostos)
        [InlineData("m^2")]          // símbolo composto
        [InlineData("SUPERLONG")]    // > 6 chars para código custom
        [InlineData("K!T")]          // caractere inválido
        public void Deve_lancar_para_valores_invalidos(string invalido)
        {
            Action act = () => new UnitOfMeasure(invalido);
            act.Should().Throw<ArgumentException>();
        }
    }
}
