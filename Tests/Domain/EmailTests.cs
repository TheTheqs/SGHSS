// Tests/Domain/EmailTests.cs

using Xunit;
using SGHSS.Domain.ValueObjects;
using FluentAssertions;


namespace SGHSS.Tests.Domain
{
    public class EmailTests
    {
        // Email deve ser normalizado para minúsculas
        // Espaços em branco no início/fim devem ser removidos
        // Email deve conter pelo menos um caractere antes e depois do '@' e um '.' após o '@'
        private const string ValidEmailRaw = "this_email@email.com";
        private const string ValidEmailMixedCaps = "ThIs_EMaiL@EMAil.coM";
        private const string ValidEmailWithSpaces = " this_email@email.com ";

        [Fact]
        public void Deve_instanciar_e_normalizar_email()
        {
            var email1 = new Email(ValidEmailRaw);
            var email2 = new Email(ValidEmailMixedCaps);
            var email3 = new Email(ValidEmailWithSpaces);
            email1.Value.Should().Be(ValidEmailRaw);
            email2.Value.Should().Be(ValidEmailRaw);
            email3.Value.Should().Be(ValidEmailRaw);
            email1.Should().Be(email2); // Igualdade por valor
            email1.Should().Be(email3); // Igualdade por valor
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]                // vazio após trim
        [InlineData("user")]               // sem '@'
        [InlineData("user@domain")]        // sem ponto após '@'
        [InlineData("user@@domain.com")]   // dois '@'
        [InlineData("user @domain.com")]   // espaço interno
        [InlineData("@domain.com")]        // nada antes do '@'
        [InlineData("user@.com")]          // ponto logo após '@'
        public void Deve_lancar_excecao_para_emails_invalidos(string invalido)
        {
            var act = () => new Email(invalido);
            act.Should().Throw<ArgumentException>();
        }

    }
}
