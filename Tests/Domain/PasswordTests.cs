// Tests/Domain/PasswordTests.cs

using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using SGHSS.Domain.ValueObjects;
using Xunit;

namespace SGHSS.Tests.Domain;

public class PasswordTests
{
    private const string ValidPassword = "abc123";
    private const string AnotherValidPassword = "xpto999";

    [Fact]
    public void Deve_criar_password_valido_e_gerar_hash()
    {
        var password = Password.Create(ValidPassword);

        password.Hash.Should().NotBeNullOrWhiteSpace();
        password.Hash.Should().NotBe(ValidPassword); // nunca armazenar senha pura
        password.ToString().Should().Be(password.Hash);
    }

    [Theory]
    [InlineData("")]
    [InlineData("     ")]
    public void Deve_lancar_excecao_para_senha_vazia(string invalida)
    {
        Action act = () => Password.Create(invalida);

        act.Should()
            .Throw<ValidationException>()
            .WithMessage("*não pode ser vazia*");
    }

    [Theory]
    [InlineData("12345")]          // menos de 6 chars
    [InlineData("abcdef")]         // só letras
    [InlineData("123456")]         // só números
    public void Deve_lancar_excecao_para_senhas_invalidas(string invalida)
    {
        Action act = () => Password.Create(invalida);

        act.Should()
            .Throw<ValidationException>();
    }

    [Fact]
    public void Verify_deve_retornar_true_para_senha_correta()
    {
        var password = Password.Create(ValidPassword);

        password.Verify(ValidPassword).Should().BeTrue();
    }

    [Fact]
    public void Verify_deve_retornar_false_para_senha_incorreta()
    {
        var password = Password.Create(ValidPassword);

        password.Verify("senhaerrada").Should().BeFalse();
    }

    [Fact]
    public void FromHash_deve_reconstruir_password_com_hash_existente()
    {
        // Arrange – cria uma senha válida para obter um hash real
        var original = Password.Create(ValidPassword);
        string hash = original.Hash;

        // Act
        var reconstructed = Password.FromHash(hash);

        // Assert
        reconstructed.Hash.Should().Be(hash);
        reconstructed.Verify(ValidPassword).Should().BeTrue();
    }

    [Fact]
    public void FromHash_deve_lancar_excecao_para_hash_invalido()
    {
        Action act = () => Password.FromHash("");

        act.Should()
            .Throw<ValidationException>()
            .WithMessage("*inválido*");
    }

    [Fact]
    public void Dois_passwords_com_mesmo_hash_devem_ser_iguais()
    {
        // Arrange
        var password1 = Password.Create(ValidPassword);
        var password2 = Password.FromHash(password1.Hash);

        // Assert
        password1.Hash.Should().Be(password2.Hash);
        password1.Should().BeEquivalentTo(password2);
    }
}
