// Tests/Domain/MedicalRecordNumberTests.cs

using FluentAssertions;
using SGHSS.Domain.ValueObjects;
using Xunit;

namespace SGHSS.Tests.Domain
{
    public class MedicalRecordNumberTests
    {
        // Valor canônico esperado (apenas A–Z0–9, letras em CAIXA ALTA)
        private const string Canonical = "MRN2025A000123";

        // Variações equivalentes de entrada (separadores e espaços serão removidos)
        private const string V1 = "MRN-2025/A-000123";
        private const string V2 = "  mrn.2025 a / 000123  ";
        private const string V3 = "mrn2025a-000123";
        private const string V4 = "MRN2025A000123";

        [Fact]
        public void Deve_instanciar_normalizar_e_manter_igualdade_por_valor()
        {
            var a = new MedicalRecordNumber(V1);
            var b = new MedicalRecordNumber(V2);
            var c = new MedicalRecordNumber(V3);
            var d = new MedicalRecordNumber(V4);

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
            var x = new MedicalRecordNumber("000123456");
            x.Value.Should().Be("000123456");
        }

        [Theory]
        [InlineData("   ")]            // vazio
        [InlineData("12345")]          // curto demais (<6)
        [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ")] // longo demais (>20)
        [InlineData("ABC_123")]        // caractere inválido "_"
        [InlineData("AAAAAAAA")]       // todos iguais
        public void Deve_lancar_para_valores_invalidos(string invalido)
        {
            Action act = () => new MedicalRecordNumber(invalido);
            act.Should().Throw<ArgumentException>();
        }
    }
}
