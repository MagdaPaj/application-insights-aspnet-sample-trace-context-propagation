using Confluent.Kafka;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TraceContextPropagationToKafka.Kafka
{
    public class KafkaConsumer<WeatherForecast> : BackgroundService
    {
        private readonly ILogger<KafkaConsumer<WeatherForecast>> _logger;
        private readonly TelemetryClient _telemetryClient;
        private readonly IConsumer<Ignore, WeatherForecast> _consumer;
        private readonly KafkaConfiguration _kafkaConfiguration;

        public KafkaConsumer(ILogger<KafkaConsumer<WeatherForecast>> logger, TelemetryClient telemetryClient, KafkaConfiguration kafkaConfiguration)
        {
            _logger = logger;
            _telemetryClient = telemetryClient;
            _kafkaConfiguration = kafkaConfiguration;

            var groupId = Guid.NewGuid().ToString();
            var config = new ConsumerConfig(_kafkaConfiguration.ClientConfig) { GroupId = groupId };
            _consumer = new ConsumerBuilder<Ignore, WeatherForecast>(config).SetValueDeserializer(new JsonSerializer<WeatherForecast>()).Build();
            _consumer.Subscribe(_kafkaConfiguration.Topic);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(() => Consume(stoppingToken));
            return Task.CompletedTask;
        }

        private void Consume(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = _consumer.Consume(stoppingToken);
                // Start telemetry tracking
                using var operation = _telemetryClient?.StartOperation<RequestTelemetry>("Consumed message from Kafka");
                _logger.LogInformation($"Received: {JsonConvert.SerializeObject(result.Message.Value)}");
                UpdateTelemetryOperationContext(result.Message.Headers, operation);
            }
        }

        private void UpdateTelemetryOperationContext(Headers headers, IOperationHolder<RequestTelemetry> operation = null)
        {
            if (operation == null)
            {
                return;
            }

            var traceparentHeader = headers.FirstOrDefault(h => h.Key == HeaderNames.TraceParent);
            if (traceparentHeader != null)
            {
                var traceparent = System.Text.Encoding.UTF8.GetString(traceparentHeader.GetValueBytes());
                try
                {
                    operation.Telemetry.Context.Operation.Id = GetOperationId(traceparent);
                    operation.Telemetry.Context.Operation.ParentId = GetOperationParentId(traceparent);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected traceparent format! Skipping update of the operation context.");
                    return;
                }
            }

            var tracestateHeader = headers.FirstOrDefault(h => h.Key == HeaderNames.TraceState);
            if (tracestateHeader != null)
            {
                var tracestate = System.Text.Encoding.UTF8.GetString(tracestateHeader.GetValueBytes());
                Activity.Current.TraceStateString = tracestate;
            }
        }

        // Traceparent is a composition of operationId and operationParentId.
        // It has the following format 00-operationId-operationParentId-00
        private static string GetOperationId(string traceparent)
        {
            return traceparent.Split('-').ElementAt(1);
        }

        private static string GetOperationParentId(string traceparent)
        {
            return traceparent.Split('-').ElementAt(2);
        }
    }
}
