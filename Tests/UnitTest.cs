// UnitTest framework used: xUnit

using FluentAssertions;
using SGHSS.Controllers;
using Xunit;

namespace SGHSS.Tests;

public class WeatherForecastUnitTests
{
    [Fact]
    public void Get_DeveRetornar_5_Itens()
    {
        var controller = new WeatherForecastController();

        var result = controller.Get();

        result.Should().NotBeNull();
        result.Count().Should().Be(5);
    }

    [Fact]
    public void Get_DatasDevemSerHojeMais1AHojeMais5_EmOrdemCrescente()
    {
        var controller = new WeatherForecastController();

        var items = controller.Get().ToArray();
        var hoje = DateOnly.FromDateTime(DateTime.Today);

        for (int i = 0; i < items.Length; i++)
            items[i].Date.Should().Be(hoje.AddDays(i + 1));

        items.Select(i => i.Date).Should().BeInAscendingOrder();
    }

    [Fact]
    public void Get_TemperaturaDentroDoIntervalo()
    {
        var controller = new WeatherForecastController();

        controller.Get()
            .Should()
            .OnlyContain(i => i.TemperatureC >= -20 && i.TemperatureC <= 55);
    }

    [Fact]
    public void Get_SummaryDeveSerUmDosValoresValidos()
    {
        var controller = new WeatherForecastController();

        var validos = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild",
            "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        controller.Get()
            .Should()
            .OnlyContain(i => validos.Contains(i.Summary));
    }
}
