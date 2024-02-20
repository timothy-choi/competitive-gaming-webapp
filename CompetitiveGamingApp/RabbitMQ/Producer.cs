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

    public void SendMerchantCreationMessage<T>(T message) {
        var connection = _factory.CreateConnection();

        using var MerchantCreationChannel = connection.CreateModel();

        MerchantCreationChannel.QueueDeclare("MerchantCreation", exclusive: false);

        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        MerchantCreationChannel.BasicPublish(exchange: "", routingKey: "MerchantCreation", body: data);
    }

    public void SendCustomerGrantRequestMessage<T>(T message) {
        var connection = _factory.CreateConnection();

        using var CustomerGrantRequestChannel = connection.CreateModel();

        CustomerGrantRequestChannel.QueueDeclare("CustomerGrantRequest", exclusive: false);

        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        CustomerGrantRequestChannel.BasicPublish(exchange: "", routingKey: "CustomerGrantRequest", body: data);
    }

    public void SendProcessPaymentMessage<T>(T message) {
        var connection = _factory.CreateConnection();

        using var ProcessPaymentChannel = connection.CreateModel();

        ProcessPaymentChannel.QueueDeclare("ProcessPayment", exclusive: false);

        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        ProcessPaymentChannel.BasicPublish(exchange: "", routingKey: "ProcessPayment", body: data);
    }

    public void SendProcessRefundMessage<T>(T message) {
        var connection = _factory.CreateConnection();

        using var ProcessRefundChannel = connection.CreateModel();

        ProcessRefundChannel.QueueDeclare("ProcessRefund", exclusive: false);

        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        ProcessRefundChannel.BasicPublish(exchange: "", routingKey: "ProcessRefund", body: data);
    }

    public void SendProcessRecordingMessage<T>(T message) {
        var connection = _factory.CreateConnection();

        using var ProcessRecordingChannel = connection.CreateModel();

        ProcessRecordingChannel.QueueDeclare("ProcessRecord", exclusive: false);

        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        ProcessRecordingChannel.BasicPublish(exchange: "", routingKey: "ProcessRecord", body: data);
    }
}