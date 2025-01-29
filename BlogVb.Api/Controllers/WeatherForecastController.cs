using BlogVb.Api.Services;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BlogVb.Api.Controllers {
	[ApiController]
	[Route("[controller]")]
	public class WeatherForecastController : ControllerBase {
		private static readonly string[] Summaries =
		[
			"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
		];

		private readonly ILogger<WeatherForecastController> _logger;

		public WeatherForecastController(ILogger<WeatherForecastController> logger) {
			_logger = logger;
		}

		[HttpGet]
		public IActionResult Get(IViewCache viewCache) {
			_logger.LogTrace("User accessed Get");
			WeatherForecast[] forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast {
				Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
				TemperatureC = Random.Shared.Next(-20, 55),
				Summary = Summaries[Random.Shared.Next(Summaries.Length)]
			})
			.ToArray();

			if(Accepts.IsJson(Request.Headers.Accept)) {
				return Content(JsonSerializer.Serialize(forecasts), Accepts.Json);
			}
			else {
				string renderedHtml = Handlebars.Compile(viewCache.GetView("pages/forecasts"))(new {
					forecasts
				});

				return Content(renderedHtml, Accepts.Html);
			}
		}
	}
}
