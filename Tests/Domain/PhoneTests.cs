using FluentAssertions;
using SGHSS.Domain.ValueObjects;
using Xunit;

namespace SGHSS.Tests.Domain
{
    public class PhoneTests
    {
        // Deve receber números de telefone válidos no Brasil
        // Deve remover todos os caracteres não numéricos
        // Deve remover espaços em branco no início/fim
        private const string ValidPhoneRaw = "11987654321"; // formato sem máscara
        private const string ValidPhoneWithMask = "(11) 98765-4321"; // formato com máscara
        private const string ValidPhoneWithSpaces = " 11987654321 "; // formato com espaços
        private const string ValidPhoneWithMaskSpecialChar = "(11)9-!8765-4//321"; // formato caracteres especiais

        [Fact]
        public void Deve_instanciar_e_normalizar_telefone()
        {
            var phone1 = new Phone(ValidPhoneRaw);
            var phone2 = new Phone(ValidPhoneWithMask);
            var phone3 = new Phone(ValidPhoneWithSpaces);
            var phone4 = new Phone(ValidPhoneWithMaskSpecialChar);
            phone1.Value.Should().Be(ValidPhoneRaw);
            phone2.Value.Should().Be(ValidPhoneRaw);
            phone3.Value.Should().Be(ValidPhoneRaw);
            phone4.Value.Should().Be(ValidPhoneRaw);
            phone1.Should().Be(phone2); // Igualdade por valor
            phone1.Should().Be(phone3); // Igualdade por valor
            phone1.Should().Be(phone4); // Igualdade por valor
        }

        [Theory]
        // Tamanho inválido
        [InlineData("123")]                  // curto demais
        [InlineData("119876543210")]        // longo demais (12 dígitos)
        [InlineData("(11) 9876-432")]       // grupos finais incompletos
        [InlineData("ab(11)98765-4321cd")]  // caracteres extras
        [InlineData("telefone")]            // texto puro
        [InlineData("++55(11)98765-4321")]  // símbolos no início
        [InlineData("   ")]                 // só espaços
        // Dígitos repetidos (rejeitado pela regra “todos iguais”)
        [InlineData("(00) 00000-0000")]
        [InlineData("11111111111")]
        public void Deve_lancar_para_valores_invalidos(string invalido)
        {
            Action act = () => new Phone(invalido);
            act.Should().Throw<ArgumentException>();
        }
    }
}
