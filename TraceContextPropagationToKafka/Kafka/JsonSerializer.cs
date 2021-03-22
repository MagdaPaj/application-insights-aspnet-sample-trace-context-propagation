using Confluent.Kafka;
using Newtonsoft.Json;
using System;
using System.Text;

namespace TraceContextPropagationToKafka.Kafka
{
    public class JsonSerializer<T> :
            ISerializer<T>, IDeserializer<T>
    {
        public byte[] Serialize(T data, SerializationContext context)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
        }

        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
        }
    }
}
