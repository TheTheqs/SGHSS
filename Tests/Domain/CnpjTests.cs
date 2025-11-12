// Tests/Domain/CnpjTests.cs

using FluentAssertions;
using SGHSS.Domain.ValueObjects;
using Xunit;

namespace SGHSS.Tests.Domain;

public class CnpjTests
{
    // Um CNPJ real válido (com DV correto)
    // 45.723.174/0001-10  (sem máscara: 45723174000110). Ambos os formatos devem ser aceitos
    private const string ValidCnpjNoMask = "45723174000110";
    private const string ValidCnpjWithMask = "45.723.174/0001-10";

    [Fact]
    public void Deve_instanciar_com_e_sem_mascara_e_ambos_devem_ser_iguais()
    {
        var cnpj1 = new Cnpj(ValidCnpjNoMask);
        var cnpj2 = new Cnpj(ValidCnpjWithMask);

        cnpj1.Value.Should().Be(ValidCnpjNoMask);
        cnpj2.Value.Should().Be(ValidCnpjNoMask);
        cnpj1.Should().Be(cnpj2); // igualdade por valor
    }

    [Fact]
    public void ToString_deve_retornar_com_mascara()
    {
        var cnpj = new Cnpj(ValidCnpjNoMask);
        cnpj.ToString().Should().Be(ValidCnpjWithMask);
    }

    [Theory]
    [InlineData("123")]                        // curto
    [InlineData("457231740001101")]            // longo
    [InlineData("45.723.174/0001-1X")]         // caractere inválido
    [InlineData("11111111000111")]             // repetidos não são válidos
    [InlineData("45723174000111")]             // DV incorreto (último dígito trocado)
    public void Parse_deve_lancar_para_valores_invalidos(string invalido)
    {
        Action act = () => new Cnpj(invalido);
        act.Should().Throw<ArgumentException>();
    }
}
