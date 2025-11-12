// Tests/Domain/CpfTests.cs

using FluentAssertions;
using SGHSS.Domain.ValueObjects;
using Xunit;

namespace SGHSS.Tests.Domain;

public class CpfTests
{
    // Um CPF real válido (com DV correto)
    // 529.982.247-25  (sem máscara: 52998224725). Ambos os formatos devem ser aceitos
    private const string ValidCpfNoMask = "52998224725";
    private const string ValidCpfWithMask = "529.982.247-25";

    [Fact]
    public void Deve_instanciar_com_e_sem_mascara_e_ambos_devem_ser_iguais()
    {
        var cpf1 = new Cpf(ValidCpfNoMask);
        var cpf2 = new Cpf(ValidCpfWithMask);

        cpf1.Value.Should().Be(ValidCpfNoMask);
        cpf2.Value.Should().Be(ValidCpfNoMask);
        cpf1.Should().Be(cpf2); // igualdade por valor
    }

    [Fact]
    public void ToString_deve_retornar_com_mascara()
    {
        var cpf = new Cpf(ValidCpfNoMask);
        cpf.ToString().Should().Be(ValidCpfWithMask);
    }

    [Theory]
    [InlineData("123")]                    // curto
    [InlineData("529982247251")]           // longo
    [InlineData("529.982.247-2X")]         // caractere inválido
    [InlineData("11111111111")]            // repetidos não são válidos
    [InlineData("52998224724")]            // DV incorreto (último dígito trocado)
    public void Parse_deve_lancar_para_valores_invalidos(string invalido)
    {
        Action act = () => new Cpf(invalido);
        act.Should().Throw<ArgumentException>();
    }
}
