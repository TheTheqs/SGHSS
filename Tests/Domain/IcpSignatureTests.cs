// Tests/Domain/IcpSignatureTests.cs

using FluentAssertions;
using SGHSS.Domain.ValueObjects;
using Xunit;

namespace SGHSS.Tests.Domain
{
    public class IcpSignatureTests
    {
        private const string CanonicalBase64 = "MIIAAg==";
        private const string WithWhitespace = "  MIIAAg==  ";
        private const string WithNewlines = "MI\nIA\nAg==";
        private const string WithPem = @"-----BEGIN PKCS7-----MIIAAg==-----END PKCS7-----";

        [Fact]
        public void Should_instantiate_normalize_and_preserve_value_equality()
        {
            var a = new IcpSignature(CanonicalBase64);
            var b = new IcpSignature(WithWhitespace);
            var c = new IcpSignature(WithNewlines);
            var d = new IcpSignature(WithPem);

            a.Value.Should().Be(CanonicalBase64);
            b.Value.Should().Be(CanonicalBase64);
            c.Value.Should().Be(CanonicalBase64);
            d.Value.Should().Be(CanonicalBase64);

            a.Should().Be(b);
            a.Should().Be(c);
            a.Should().Be(d);

            a.ToString().Should().Be(CanonicalBase64);
        }

        [Theory]
        [InlineData("   ")]        // Vazio
        [InlineData("not-base64")] // Não é Base64
        [InlineData("0xDEADBEEF")] // Formato inválido
        [InlineData("====")]       // Padding inválido
        [InlineData("AQID")]       // Não é DER (não começa com 0x30)
        public void Should_throw_for_invalid_values(string invalid)
        {
            Action act = () => new IcpSignature(invalid);
            act.Should().Throw<ArgumentException>();
        }
    }
}
