using System.Net;
using System.Net.Http.Json;
using stock_api;

namespace stock_api.Tests
{
    public class WeatherForecastTests : IClassFixture<CustomWebApplicationFactory>
    {
        // Mirrors WeatherForecastController's private Summaries array - these are stable
        // invariants of the endpoint even though the values it returns are randomized.
        private static readonly string[] KnownSummaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly HttpClient _client;

        public WeatherForecastTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_ReturnsOk_WithFiveForecasts_NoAuthRequired()
        {
            var response = await _client.GetAsync("/WeatherForecast");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var forecasts = await response.Content.ReadFromJsonAsync<List<WeatherForecast>>();

            Assert.NotNull(forecasts);
            Assert.Equal(5, forecasts!.Count);
        }

        [Fact]
        public async Task Get_ReturnsForecasts_WithTemperatureAndSummaryWithinKnownRanges()
        {
            var response = await _client.GetAsync("/WeatherForecast");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var forecasts = await response.Content.ReadFromJsonAsync<List<WeatherForecast>>();

            Assert.NotNull(forecasts);
            foreach (var forecast in forecasts!)
            {
                Assert.InRange(forecast.TemperatureC, -20, 54);
                Assert.Contains(forecast.Summary, KnownSummaries);
            }
        }
    }
}
