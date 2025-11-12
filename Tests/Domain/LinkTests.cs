// Tests/Domain/LinkTests.cs

using FluentAssertions;
using SGHSS.Domain.ValueObjects;
using Xunit;

namespace SGHSS.Tests.Domain
{
    public class LinkTests
    {
        // Valor normalizado esperado: apenas o path + query + fragment, SEM domínio e SEM a barra inicial.
        // Observação: mantemos o case do path para não quebrar cases sensíveis!
        private const string NormalizedPath = "teleconsulta/ABC?x=1#sala";

        // Variações de entrada equivalentes:
        private const string UrlHttpsWww = "https://www.hospital.com/teleconsulta/ABC?x=1#sala";
        private const string UrlHttpsNoWww = "https://hospital.com/teleconsulta/ABC?x=1#sala";
        private const string UrlHttp = "http://hospital.com/teleconsulta/ABC?x=1#sala";
        private const string UrlSubdomain = "https://tele.hospital.com/teleconsulta/ABC?x=1#sala";
        private const string UrlNoScheme = "www.hospital.com/teleconsulta/ABC?x=1#sala";
        private const string UrlRelative = "/teleconsulta/ABC?x=1#sala";
        private const string UrlWithSpaces = "   https://hospital.com/teleconsulta/ABC?x=1#sala   ";
        private const string UrlMultiSlashes = "https://hospital.com//teleconsulta///ABC?x=1#sala";

        [Fact]
        public void Deve_instanciar_e_normalizar_link_para_apenas_o_path()
        {
            var l0 = new Link(NormalizedPath);
            var l1 = new Link(UrlHttpsWww);
            var l2 = new Link(UrlHttpsNoWww);
            var l3 = new Link(UrlHttp);
            var l4 = new Link(UrlSubdomain);
            var l5 = new Link(UrlNoScheme);
            var l6 = new Link(UrlRelative);
            var l7 = new Link(UrlWithSpaces);
            var l8 = new Link(UrlMultiSlashes);

            l0.Value.Should().Be(NormalizedPath);
            l1.Value.Should().Be(NormalizedPath);
            l2.Value.Should().Be(NormalizedPath);
            l3.Value.Should().Be(NormalizedPath);
            l4.Value.Should().Be(NormalizedPath);
            l5.Value.Should().Be(NormalizedPath);
            l6.Value.Should().Be(NormalizedPath);
            l7.Value.Should().Be(NormalizedPath);
            l8.Value.Should().Be(NormalizedPath);

            // Igualdade por valor
            l1.Should().Be(l0);
            l1.Should().Be(l2);
            l1.Should().Be(l3);
            l1.Should().Be(l4);
            l1.Should().Be(l5);
            l1.Should().Be(l6);
            l1.Should().Be(l7);
            l1.Should().Be(l8);
        }

        [Fact]
        public void ToString_deve_retornar_o_mesmo_path_normalizado()
        {
            var link = new Link(UrlHttpsWww);
            link.ToString().Should().Be(NormalizedPath);
        }

        [Theory]
        [InlineData("   ")]                 // vazio / espaços
        [InlineData("http://hospital.com")] // domínio absoluto sem path
        [InlineData("www.hospital.com")]    // domínio sem path
        [InlineData("////")]                // só barras (path efetivo vazio após normalização)
        public void Deve_lancar_para_valores_invalidos(string invalido)
        {
            Action act = () => new Link(invalido);
            act.Should().Throw<ArgumentException>();
        }
    }
}
