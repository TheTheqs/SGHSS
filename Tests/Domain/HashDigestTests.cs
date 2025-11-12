// Tests/Domain/HashDigestTests.cs

using FluentAssertions;
using SGHSS.Domain.ValueObjects;
using Xunit;

namespace SGHSS.Tests.Domain
{
    public class HashDigestTests
    {
        // SHA-256 (64 hex) em minúsculas — forma canônica esperada
        private const string Canonical =
            "9f2c8a1f4b6d7e8091a2b3c4d5e6f708112233445566778899aabbccddeeff00";

        // Variações equivalentes:
        private const string Uppercase =
            "9F2C8A1F4B6D7E8091A2B3C4D5E6F708112233445566778899AABBCCDDEEFF00";
        private const string With0x =
            "0x9f2c8a1f4b6d7e8091a2b3c4d5e6f708112233445566778899aabbccddeeff00";
        private const string WithSeparators =
            "9f:2c:8a:1f:4b:6d:7e:80-91-a2-b3-c4-d5-e6-f7-08-11-22-33-44-55-66-77-88-99-aa-bb-cc-dd-ee-ff-00";
        private const string WithSpaces =
            "  9f2c8a1f4b6d7e8091a2b3c4d5e6f708112233445566778899aabbccddeeff00  ";

        [Fact]
        public void Deve_instanciar_normalizar_e_manter_igualdade_por_valor()
        {
            var a = new HashDigest(Canonical);
            var b = new HashDigest(Uppercase);
            var c = new HashDigest(With0x);
            var d = new HashDigest(WithSeparators);
            var e = new HashDigest(WithSpaces);

            a.Value.Should().Be(Canonical);
            b.Value.Should().Be(Canonical);
            c.Value.Should().Be(Canonical);
            d.Value.Should().Be(Canonical);
            e.Value.Should().Be(Canonical);

            a.Should().Be(b);
            a.Should().Be(c);
            a.Should().Be(d);
            a.Should().Be(e);

            a.ToString().Should().Be(Canonical);
        }

        [Theory]
        [InlineData("   ")]                                      // vazio
        [InlineData("xyz")]                                      // não-hex
        [InlineData("0x9f2c")]                                   // tamanho inválido
        [InlineData("zz2c8a1f4b6d7e8091a2b3c4d5e6f70811223344")] // caractere inválido 'z'
        [InlineData("::::----")]                                 // apenas separadores → vira vazio
        public void Deve_lancar_para_valores_invalidos(string invalido)
        {
            Action act = () => new HashDigest(invalido);
            act.Should().Throw<ArgumentException>();
        }
    }
}
