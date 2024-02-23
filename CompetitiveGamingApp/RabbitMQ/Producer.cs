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

    public void SendGenerateScheduleMessage<T>(T message) {
        var connection = _factory.CreateConnection();

        using var ProcessGenerateScheduleChannel = connection.CreateModel();

        ProcessGenerateScheduleChannel.QueueDeclare("GenerateSchedule", exclusive: false);

        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        ProcessGenerateScheduleChannel.BasicPublish(exchange: "", routingKey: "GenerateSchedule", body: data);
    }

    public void SendProcessSubmittedScheduleMessage<T>(T message) {
        var connection = _factory.CreateConnection();

        using var ProcessSubmittedScheduleChannel = connection.CreateModel();

        ProcessSubmittedScheduleChannel.QueueDeclare("SubmittedSchedule", exclusive: false);

        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        ProcessSubmittedScheduleChannel.BasicPublish(exchange: "", routingKey: "SubmittedSchedule", body: data);
    }

    public void SendProcessGeneratedScheduleMessage<T>(T message) {
        var connection = _factory.CreateConnection();

        using var ProcessGeneratedScheduleChannel = connection.CreateModel();

        ProcessGeneratedScheduleChannel.QueueDeclare("ProcessedSchedule", exclusive: false);

        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        ProcessGeneratedScheduleChannel.BasicPublish(exchange: "", routingKey: "ProcessedSchedule", body: data);
    }

    public void SendProcessUserSubmittedWholeModeMessage<T>(T message) {
        var connection = _factory.CreateConnection();

        using var ProcessUserSubmittedWholeModeChannel = connection.CreateModel();

        ProcessUserSubmittedWholeModeChannel.QueueDeclare("ProcessUserSubmittedWholeMode", exclusive: false);

        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        ProcessUserSubmittedWholeModeChannel.BasicPublish(exchange: "", routingKey: "ProcessUserSubmittedWholeMode", body: data);
    }

    public void SendCreateWholeModeOrderingMessage<T>(T message) {
        var connection = _factory.CreateConnection();

        using var ProcessCreateWholeModeOrderingChannel = connection.CreateModel();

        ProcessCreateWholeModeOrderingChannel.QueueDeclare("ProcessCreateWholeModeOrdering", exclusive: false);

        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        ProcessCreateWholeModeOrderingChannel.BasicPublish(exchange: "", routingKey: "ProcessCreateWholeModeOrdering", body: data);
    }
}