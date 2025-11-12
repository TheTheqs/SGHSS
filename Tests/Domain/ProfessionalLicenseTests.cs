// Tests/Domain/ProfessionalLicenseTests.cs

using FluentAssertions;
using SGHSS.Domain.ValueObjects;
using Xunit;

namespace SGHSS.Tests.Domain
{
    public class ProfessionalLicenseTests
    {
        // Caso base (CRM-SP), número sem zeros à esquerda
        private const string Canonical = "CRM-SP 123456";

        // Variações equivalentes de entrada:
        private const string V1 = "CRM-SP 123456";
        private const string V2 = "crm sp 123456";
        private const string V3 = "CRM/SP-123456";
        private const string V4 = "CRM-SP123456";
        private const string V5 = "123456 SP CRM";
        private const string V6 = "  CRM  -  sp   123456  ";

        [Fact]
        public void Deve_instanciar_normalizar_e_manter_igualdade_por_valor_para_CRM_SP()
        {
            var p1 = new ProfessionalLicense(V1);
            var p2 = new ProfessionalLicense(V2);
            var p3 = new ProfessionalLicense(V3);
            var p4 = new ProfessionalLicense(V4);
            var p5 = new ProfessionalLicense(V5);
            var p6 = new ProfessionalLicense(V6);

            p1.Value.Should().Be(Canonical);
            p2.Value.Should().Be(Canonical);
            p3.Value.Should().Be(Canonical);
            p4.Value.Should().Be(Canonical);
            p5.Value.Should().Be(Canonical);
            p6.Value.Should().Be(Canonical);

            // Igualdade por valor
            p1.Should().Be(p2);
            p1.Should().Be(p3);
            p1.Should().Be(p4);
            p1.Should().Be(p5);
            p1.Should().Be(p6);

            // ToString deve retornar o formato canônico
            p1.ToString().Should().Be(Canonical);
        }

        // Caso com zeros à esquerda no número (deve preservar)
        private const string CanonicalLeadingZeros = "CRM-SP 012345";
        private const string L1 = "crm/sp-012345";
        private const string L2 = "CRM-SP 00012345";
        private const string L3 = "012345 sp crm";
        private const string L4 = "  crm - Sp   012345 ";

        [Fact]
        public void Deve_preservar_zeros_a_esquerda_e_normalizar_case_de_conselho_e_uf()
        {
            var a = new ProfessionalLicense(L1);
            var b = new ProfessionalLicense(L3);
            var c = new ProfessionalLicense(L4);

            a.Value.Should().Be(CanonicalLeadingZeros);
            b.Value.Should().Be(CanonicalLeadingZeros);
            c.Value.Should().Be(CanonicalLeadingZeros);

            a.Should().Be(b);
            a.Should().Be(c);

            a.ToString().Should().Be(CanonicalLeadingZeros);
        }

        // Exemplo com outro conselho/UF para garantir generalidade
        private const string CanonicalCorem = "COREN-RJ 98765";
        private const string C1 = "coren rj 98765";
        private const string C2 = "COREN/RJ-98765";
        private const string C3 = "98765 rj coren";

        [Fact]
        public void Deve_funcionar_para_outros_conselhos_e_ufs()
        {
            var x = new ProfessionalLicense(C1);
            var y = new ProfessionalLicense(C2);
            var z = new ProfessionalLicense(C3);

            x.Value.Should().Be(CanonicalCorem);
            y.Value.Should().Be(CanonicalCorem);
            z.Value.Should().Be(CanonicalCorem);

            x.Should().Be(y);
            x.Should().Be(z);

            x.ToString().Should().Be(CanonicalCorem);
        }

        [Theory]
        [InlineData("CRM COREN SP 123456")]   // dois conselhos
        [InlineData("CRM SP RJ 123456")]      // duas UFs
        [InlineData("CRM SP 123456 7890")]    // dois números
        [InlineData("CRM XX 123456")]         // UF inválida
        [InlineData("ABC-SP 123456")]         // conselho fora da whitelist
        public void Deve_lancar_para_valores_invalidos(string invalido)
        {
            Action act = () => new ProfessionalLicense(invalido);
            act.Should().Throw<ArgumentException>();
        }
    }
}
