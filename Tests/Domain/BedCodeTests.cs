// Tests/Domain/BedCodeTests.cs

using FluentAssertions;
using SGHSS.Domain.ValueObjects;
using Xunit;

namespace SGHSS.Tests.Domain
{
    public class BedCodeTests
    {
        // Valor canônico: apenas A–Z0–9, letras em CAIXA ALTA, sem separadores
        private const string Canonical = "UTI01A23";

        // Variações equivalentes de entrada (separadores/espacos removidos)
        private const string V1 = "UTI-01/A-23";
        private const string V2 = "  uti 01 a 23  ";
        private const string V3 = "UTI01-A23";
        private const string V4 = "UTI.01 A/23";

        [Fact]
        public void Deve_instanciar_normalizar_e_manter_igualdade_por_valor()
        {
            var a = new BedCode(V1);
            var b = new BedCode(V2);
            var c = new BedCode(V3);
            var d = new BedCode(V4);

            a.Value.Should().Be(Canonical);
            b.Value.Should().Be(Canonical);
            c.Value.Should().Be(Canonical);
            d.Value.Should().Be(Canonical);

            a.Should().Be(b);
            a.Should().Be(c);
            a.Should().Be(d);

            a.ToString().Should().Be(Canonical);
        }

        [Fact]
        public void Deve_preservar_zeros_a_esquerda()
        {
            var x = new BedCode("00-A1");
            x.Value.Should().Be("00A1");
        }

        [Theory]
        [InlineData("   ")]        // vazio
        [InlineData("A")]          // curto demais (<2)
        [InlineData("AAAAAAAA")]   // todos iguais
        [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ")] // longo demais (>20, por exemplo)
        public void Deve_lancar_para_valores_invalidos(string invalido)
        {
            Action act = () => new BedCode(invalido);
            act.Should().Throw<ArgumentException>();
        }
    }
}
