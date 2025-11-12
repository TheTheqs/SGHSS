// Tests/Domain/TimeZoneTests.cs

using FluentAssertions;
using Xunit;
using TimeZone = SGHSS.Domain.ValueObjects.TimeZone; // evitar conflito com System.TimeZoneInfo

namespace SGHSS.Tests.Domain
{
    public class TimeZoneTests
    {
        private const string IanaCanonical = "America/Sao_Paulo";
        private const string IanaWithSpaces = "  America/Sao_Paulo  ";

        [Fact]
        public void Deve_instanciar_e_normalizar_IANA_preservando_case()
        {
            var tz1 = new TimeZone(IanaCanonical);
            var tz2 = new TimeZone(IanaWithSpaces);

            tz1.Value.Should().Be(IanaCanonical);
            tz2.Value.Should().Be(IanaCanonical);

            // igualdade por valor
            tz1.Should().Be(tz2);

            // ToString retorna o Value
            tz1.ToString().Should().Be(IanaCanonical);
        }

        [Fact]
        public void Deve_tratar_UTC_e_offsets_de_forma_canonica()
        {
            // UTC aliases -> Etc/UTC
            var u1 = new TimeZone("UTC");
            var u2 = new TimeZone("Etc/UTC");
            var u3 = new TimeZone("Z");

            u1.Value.Should().Be("Etc/UTC");
            u2.Value.Should().Be("Etc/UTC");
            u3.Value.Should().Be("Etc/UTC");

            u1.Should().Be(u2);
            u1.Should().Be(u3);

            // Offsets equivalentes -> UTC+03:00
            var o1 = new TimeZone("+03:00");
            var o2 = new TimeZone("UTC+03:00");

            o1.Value.Should().Be("UTC+03:00");
            o2.Value.Should().Be("UTC+03:00");

            o1.Should().Be(o2);

            o1.ToString().Should().Be("UTC+03:00");
        }

        [Theory]
        [InlineData("   ")]                 // vazio / apenas espaços
        [InlineData("UTC+25:00")]           // horas > 14
        [InlineData("UTC-12:60")]           // minutos > 59
        [InlineData("America//Sao_Paulo")]  // segmento vazio (barra dupla)
        [InlineData("America/São_Paulo")]   // caractere não-ASCII no ID IANA
        [InlineData("http://example.com")]  // formato indevido (não é IANA nem offset)
        public void Deve_lancar_para_valores_invalidos(string invalido)
        {
            Action act = () => new TimeZone(invalido);
            act.Should().Throw<ArgumentException>();
        }
    }
}
