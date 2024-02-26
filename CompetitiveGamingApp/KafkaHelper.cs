namespace KafkaHelper;

using Confluent.Kafka;


public class KafkaProducer {
    private readonly ProducerConfig _config;

    public KafkaProducer() {
        _config = new ProducerConfig {
            BootstrapServers = ""
        };
    }
    
    public async Task ProduceMessageAsync(string topic, string message)
    {
        using (var producer = new ProducerBuilder<Null, string>(_config).Build())
        {
            await producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
        }
    }
}