// Tests/Domain/IpAddressTests.cs

using FluentAssertions;
using SGHSS.Domain.ValueObjects;
using System.Net;
using Xunit;

namespace SGHSS.Tests.Domain
{
    public class IpAddressTests
    {
        // IPv4: entradas equivalentes devem normalizar para "192.168.0.1"
        private const string Ipv4Canonical = "192.168.0.1";
        private const string Ipv4WithLeadingZeros = "192.168.000.001";
        private const string Ipv4WithSpaces = "  192.168.0.1  ";

        // IPv6: entradas equivalentes devem normalizar para "2001:db8::ff00:42:8329"
        private const string Ipv6Canonical = "2001:db8::ff00:42:8329";
        private const string Ipv6Full = "2001:0db8:0000:0000:0000:ff00:0042:8329";
        private const string Ipv6Upper = "2001:DB8:0:0:0:FF00:42:8329";
        private const string Ipv6Mixed = "2001:db8:0:0::ff00:0042:8329";

        [Fact]
        public void Deve_instanciar_e_normalizar_IPv4()
        {
            var ip1 = new IpAddress(Ipv4Canonical);
            var ip2 = new IpAddress(Ipv4WithLeadingZeros);
            var ip3 = new IpAddress(Ipv4WithSpaces);

            ip1.Value.Should().Be(Ipv4Canonical);
            ip2.Value.Should().Be(Ipv4Canonical);
            ip3.Value.Should().Be(Ipv4Canonical);

            ip1.Should().Be(ip2); // igualdade por valor
            ip1.Should().Be(ip3);

            ip1.ToString().Should().Be(Ipv4Canonical);
        }

        [Fact]
        public void Deve_instanciar_e_normalizar_IPv6_e_igualdade_por_valor()
        {
            var v1 = new IpAddress(Ipv6Canonical);
            var v2 = new IpAddress(Ipv6Full);
            var v3 = new IpAddress(Ipv6Upper);
            var v4 = new IpAddress(Ipv6Mixed);

            v1.Value.Should().Be(Ipv6Canonical);
            v2.Value.Should().Be(Ipv6Canonical);
            v3.Value.Should().Be(Ipv6Canonical);
            v4.Value.Should().Be(Ipv6Canonical);

            v1.Should().Be(v2); // igualdade por valor
            v1.Should().Be(v3);
            v1.Should().Be(v4);

            v1.ToString().Should().Be(Ipv6Canonical);
        }

        [Theory]
        [InlineData("   ")]                  // vazio / apenas espaços
        [InlineData("999.999.999.999")]      // IPv4 fora de faixa
        [InlineData("256.0.0.1")]            // IPv4 com octeto inválido (>255)
        [InlineData("2001:::1")]             // IPv6 malformado (três ':')
        [InlineData("abcd")]                 // texto aleatório
        [InlineData("192.168.1")]            // IPv4 incompleto
        public void Deve_lancar_para_valores_invalidos(string invalido)
        {
            Action act = () => new IpAddress(invalido);
            act.Should().Throw<ArgumentException>();
        }

    }
}
