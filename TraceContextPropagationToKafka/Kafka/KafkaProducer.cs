using Confluent.Kafka;
using System;
using System.Diagnostics;
using System.Text;
using TraceContextPropagationToKafka.Kafka;

namespace TraceContextPropagationToKafka
{
    public class KafkaProducer<WeatherForecast> : IDisposable
    {
        private readonly IProducer<Null, WeatherForecast> _producer;

        public KafkaProducer(KafkaConfiguration kafkaConfiguration)
        {
            var producerConfig = new ProducerConfig(kafkaConfiguration.ClientConfig);
            _producer = new ProducerBuilder<Null, WeatherForecast>(producerConfig)
                .SetValueSerializer(new JsonSerializer<WeatherForecast>())
                .Build();
        }

        public void Produce(string topic, WeatherForecast message)
        {
            var msg = new Message<Null, WeatherForecast>
            {
                Value = message
            };

            var headers = Activity.Current?.PopulateHeaders();

            foreach (var kvp in headers)
            {
                if (msg.Headers == null)
                {
                    msg.Headers = new Headers();
                }
                msg.Headers.Add(kvp.Key, Encoding.UTF8.GetBytes(kvp.Value));
            }

            _producer.Produce(topic, msg);
        }

        public void Dispose()
        {
            _producer.Dispose();
        }
    }
}
