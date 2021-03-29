using Confluent.Kafka;

namespace TraceContextPropagationToKafka.Kafka
{
    public class KafkaConfiguration
    {
        public ClientConfig ClientConfig { get; set; } = new ClientConfig();
        public string Topic { get; set; }
    }

}
