using Photobiz.Application.Features.WeatherForecasts.GetWeatherForecasts;

namespace Photobiz.Application.Tests.Features.WeatherForecasts.GetWeatherForecasts
{
    public class GetWeatherForecastsQueryHandlerTests
    {
        private readonly GetWeatherForecastsQueryHandler _handler = new();

        [Fact]
        public async Task Handle_ReturnsFiveForecasts()
        {
            var result = await _handler.Handle(new GetWeatherForecastsQuery(), CancellationToken.None);

            Assert.Equal(5, result.Count);
        }

        [Fact]
        public async Task Handle_ReturnsForecastsWithFutureDates()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var result = await _handler.Handle(new GetWeatherForecastsQuery(), CancellationToken.None);

            Assert.All(result, forecast => Assert.True(forecast.Date > today));
        }

        [Fact]
        public async Task Handle_ReturnsForecastsWithConsistentFahrenheitConversion()
        {
            var result = await _handler.Handle(new GetWeatherForecastsQuery(), CancellationToken.None);

            Assert.All(result, forecast =>
                Assert.Equal(32 + (int)(forecast.TemperatureC / 0.5556), forecast.TemperatureF));
        }
    }
}
