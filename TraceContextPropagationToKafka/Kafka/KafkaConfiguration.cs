using Confluent.Kafka;

namespace TraceContextPropagationToKafka.Kafka
{
    public class KafkaConfiguration
    {
        public ClientConfig ClientConfig { get; set; } = new ClientConfig();
        public KafkaTopics Topics { get; set; } = new KafkaTopics();
    }

    public class KafkaTopics
    {
        public string Demo { get; set; } = null!;
    }
}
