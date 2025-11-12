// Tests/Domain/PeriodTests.cs

using FluentAssertions;
using SGHSS.Domain.ValueObjects;
using Xunit;

namespace SGHSS.Tests.Domain
{
    public class PeriodTests
    {
        [Fact]
        public void Deve_instanciar_normalizar_e_manter_igualdade_por_valor()
        {
            var start = new DateOnly(2025, 11, 1);
            var end = new DateOnly(2025, 11, 30);

            var p = new Period(start, end);

            p.Start.Should().Be(start);
            p.End.Should().Be(end);
            p.Value.Should().Be("2025-11-01..2025-11-30");
            p.ToString().Should().Be("2025-11-01..2025-11-30");
        }

        [Fact]
        public void Deve_aceitar_valores_de_dia_unico()
        {
            var d = new DateOnly(2025, 11, 12);
            var p = new Period(d, d);

            p.Start.Should().Be(d);
            p.End.Should().Be(d);
            p.Value.Should().Be("2025-11-12..2025-11-12");
        }

        [Fact]
        public void ForMonth_should_create_canonical_month_period()
        {
            var pm = Period.ForMonth(2025, 11);

            pm.Start.Should().Be(new DateOnly(2025, 11, 1));
            pm.End.Should().Be(new DateOnly(2025, 11, 30));
            pm.Value.Should().Be("2025-11-01..2025-11-30");

            var p2 = new Period(new DateOnly(2025, 11, 1), new DateOnly(2025, 11, 30));
            pm.Should().Be(p2);
        }

        [Theory]
        [InlineData(2025, 11, 10, 2025, 11, 9)]  // início > fim
        [InlineData(2025, 12, 31, 2025, 1, 1)]   // início > fim
        public void Deve_lancar_excecao_para_inicio_depois_de_fim(
    int y1, int m1, int d1, int y2, int m2, int d2)
        {
            var start = new DateOnly(y1, m1, d1);
            var end = new DateOnly(y2, m2, d2);

            Action act = () => new Period(start, end);
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData(2025, 0)]   // mês inválido
        [InlineData(2025, 13)]  // mês inválido
        public void Deve_lancar_excecao_para_mes_invalido(int year, int month)
        {
            Action act = () => Period.ForMonth(year, month);
            act.Should().Throw<ArgumentException>();
        }
    }
}
