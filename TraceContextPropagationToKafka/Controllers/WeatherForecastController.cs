using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using TraceContextPropagationToKafka.Kafka;

namespace TraceContextPropagationToKafka.Controllers
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
        private readonly KafkaConfiguration _kafkaConfiguration;
        private readonly KafkaProducer<WeatherForecast> _producer;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, KafkaConfiguration kafkaConfiguration, KafkaProducer<WeatherForecast> producer)
        {
            _logger = logger;
            _kafkaConfiguration = kafkaConfiguration;
            _producer = producer;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost]
        public ActionResult PostWeatherForecast(WeatherForecast weatherForecast)
        {
            _producer.Produce(_kafkaConfiguration.Topics.Demo, weatherForecast, new Dictionary<string, string> { { "foo", "bar"} });

            return Content("OK");
        }
    }
}
