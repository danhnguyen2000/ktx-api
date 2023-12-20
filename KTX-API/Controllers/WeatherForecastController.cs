using Microsoft.AspNetCore.Mvc;
using KTX_DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace KTX_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly DBContext _context;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, DBContext test_KtxContext)
        {
            _logger = logger;
            _context = test_KtxContext;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("test")]
        public async Task<ActionResult> GetAll()
        {
            var data = await _context.Tests.ToListAsync();

            return Ok(new DefaultResponseContext { Message = "Success", Data = data });
        }
    }
}