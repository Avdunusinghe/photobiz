using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Photobiz.Application.Features.WeatherForecasts.GetWeatherForecasts;

namespace Photobiz.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WeatherForecastController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<ActionResult<IReadOnlyList<WeatherForecastDto>>> Get(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetWeatherForecastsQuery(), cancellationToken);
            return Ok(result);
        }
    }
}
