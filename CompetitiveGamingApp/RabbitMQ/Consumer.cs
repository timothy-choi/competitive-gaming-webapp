using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;

namespace CompetitiveGamingApp.RabbitMQ;

public class Consumer {
     private readonly ConnectionFactory _factory;

     public Consumer() {
        _factory = new ConnectionFactory {
            HostName = "localhost"
        };
    }

    public dynamic RecieveMerchantCreationMessage() {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "MerchantCreation", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        Dictionary<string, object> MerchantCreationInfo = new Dictionary<string, object>();

        consumer.Received += (model, ea) => {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(message)!;

            MerchantCreationInfo = data;
            
            channel.BasicAck(ea.DeliveryTag, false);
        };

        channel.BasicConsume(queue: "MerchantCreation", autoAck: false, consumer: consumer);

        return MerchantCreationInfo;
    }

    public dynamic RecieveCustomerGrantRequestMessage() {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "CustomerGrantRequest", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        Dictionary<string, object> CustomerGrantRequestInfo = new Dictionary<string, object>();

        consumer.Received += (model, ea) => {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(message)!;

            CustomerGrantRequestInfo = data;
            
            channel.BasicAck(ea.DeliveryTag, false);
        };

        channel.BasicConsume(queue: "CustomerGrantRequest", autoAck: false, consumer: consumer);

        return CustomerGrantRequestInfo;
    }

    public dynamic RecieveProcessPaymentMessage() {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "ProcessPayment", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        Dictionary<string, object> ProcessPaymentInfo = new Dictionary<string, object>();

        consumer.Received += (model, ea) => {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(message)!;

            ProcessPaymentInfo = data;
            
            channel.BasicAck(ea.DeliveryTag, false);
        };

        channel.BasicConsume(queue: "ProcessPayment", autoAck: false, consumer: consumer);

        return ProcessPaymentInfo;
    }

    public dynamic RecieveProcessRefundMessage() {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "ProcessRefund", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        Dictionary<string, object> ProcessRefundInfo = new Dictionary<string, object>();

        consumer.Received += (model, ea) => {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(message)!;

            ProcessRefundInfo = data;
            
            channel.BasicAck(ea.DeliveryTag, false);
        };

        channel.BasicConsume(queue: "ProcessRefund", autoAck: false, consumer: consumer);

        return ProcessRefundInfo;
    }
}