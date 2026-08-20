using MediatR;

namespace Photobiz.Application.Features.WeatherForecasts.GetWeatherForecasts
{
    public class GetWeatherForecastsQueryHandler
        : IRequestHandler<GetWeatherForecastsQuery, IReadOnlyList<WeatherForecastDto>>
    {
        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        public Task<IReadOnlyList<WeatherForecastDto>> Handle(
            GetWeatherForecastsQuery request,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<WeatherForecastDto> forecasts = Enumerable.Range(1, 5)
                .Select(index => new WeatherForecastDto(
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    Summaries[Random.Shared.Next(Summaries.Length)]))
                .ToArray();

            return Task.FromResult(forecasts);
        }
    }
}
