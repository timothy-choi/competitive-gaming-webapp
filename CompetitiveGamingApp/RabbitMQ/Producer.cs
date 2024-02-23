using System.Text;
using RabbitMQ.Client;
using Newtonsoft.Json;


namespace CompetitiveGamingApp.RabbitMQ;


public class Producer {
    private readonly ConnectionFactory _factory;
    public Producer() {
        _factory = new ConnectionFactory {
            HostName = "localhost"
        };
    }

    public void SendMessage(string queue, Dictionary<string, object> message) {
        var connection = _factory.CreateConnection();

        using var MerchantCreationChannel = connection.CreateModel();

        MerchantCreationChannel.QueueDeclare(queue, exclusive: false);

        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        MerchantCreationChannel.BasicPublish(exchange: "", routingKey: queue, body: data);
    }
}