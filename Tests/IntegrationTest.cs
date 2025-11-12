// IntegrationTest for ASP.NET Core Web API using xUnit and FluentAssertions

using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Xunit;

namespace SGHSS.Tests;

public class WeatherForecastIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WeatherForecastIntegrationTests(WebApplicationFactory<Program> baseFactory)
    {
        var contentRoot = Directory.GetCurrentDirectory();

        _factory = baseFactory.WithWebHostBuilder(builder =>
        {
            builder.UseContentRoot(contentRoot);
        });
    }

    [Fact]
    public async Task GetWeatherForecast_DeveRetornar200EListaCom5Itens()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/WeatherForecast");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<WeatherForecastDto[]>();
        payload.Should().NotBeNull();
        payload!.Length.Should().Be(5);

        payload.All(p => p.TemperatureC is >= -20 and <= 55).Should().BeTrue();
        payload.Select(p => p.Date).Should().BeInAscendingOrder();
    }

    private record WeatherForecastDto(DateOnly Date, int TemperatureC, string Summary);
}
