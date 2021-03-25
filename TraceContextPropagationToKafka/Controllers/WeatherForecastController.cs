using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.Diagnostics;
using TraceContextPropagationToKafka.Kafka;

namespace TraceContextPropagationToKafka.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly KafkaConfiguration _kafkaConfiguration;
        private readonly KafkaProducer<WeatherForecast> _producer;
        private readonly TelemetryClient _telemetryClient;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, KafkaConfiguration kafkaConfiguration, KafkaProducer<WeatherForecast> producer, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _kafkaConfiguration = kafkaConfiguration;
            _producer = producer;
            _telemetryClient = telemetryClient;
        }

        [HttpPost]
        public ActionResult Post(WeatherForecast weatherForecast)
        {
            var topic = _kafkaConfiguration.Topic;

            using (var operation = _telemetryClient.StartOperation<DependencyTelemetry>($"Produce to Kafka topic {topic}"))
            {
                operation.Telemetry.Type = "Kafka Broker";
                var headers = PopulateHeaders();
                _producer.Produce(topic, weatherForecast, headers);
                _logger.LogInformation("Message successfully produced to Kafka");
            };

            return Content("OK");
        }

        private static Dictionary<string, string> PopulateHeaders()
        {
            var headers = new Dictionary<string, string>();
            var traceparent = GetTraceparent();
            if (!string.IsNullOrEmpty(traceparent))
            {
                headers.Add(HeaderNames.TraceParent, traceparent);
            }

            var tracestate = GetTracestate();
            if (!string.IsNullOrEmpty(tracestate))
            {
                headers.Add(HeaderNames.TraceState, tracestate);
            }

            return headers;
        }

        private static string GetTraceparent()
        {
            return !string.IsNullOrEmpty(Activity.Current?.Id) ? Activity.Current.Id : string.Empty;
        }
        private static string GetTracestate()
        {
            return !string.IsNullOrEmpty(Activity.Current?.TraceStateString) ? Activity.Current.TraceStateString : string.Empty;
        }
    }
}
