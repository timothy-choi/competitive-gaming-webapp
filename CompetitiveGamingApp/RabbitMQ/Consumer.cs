using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.SignalR;

namespace CompetitiveGamingApp.RabbitMQ;

public class Consumer {
     private readonly ConnectionFactory _factory;

     private readonly NotifyRequest _req;

     public Consumer() {
        _factory = new ConnectionFactory {
            HostName = "localhost"
        };

        _req = new NotifyRequest();
    }

    public async Task RecieveMerchantCreationMessage() {
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

        await _req.NotifyToCallRequest("MerchantCreation", MerchantCreationInfo);
    }

    public async Task RecieveCustomerGrantRequestMessage() {
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

        await _req.NotifyToCallRequest("CustomerGrantRequest", CustomerGrantRequestInfo);
    }

    public async Task RecieveProcessPaymentMessage() {
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

        await _req.NotifyToCallRequest("ProcessPayment", ProcessPaymentInfo);
    }

    public async Task RecieveProcessRefundMessage() {
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

        await _req.NotifyToCallRequest("ProcessRefund", ProcessRefundInfo);
    }

    public async Task RecieveProcessRecordMessage() {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "ProcessRecord", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        Dictionary<string, object> ProcessRecordInfo = new Dictionary<string, object>();

        consumer.Received += (model, ea) => {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(message)!;

            ProcessRecordInfo = data;
            
            channel.BasicAck(ea.DeliveryTag, false);
        };

        channel.BasicConsume(queue: "ProcessRecord", autoAck: false, consumer: consumer);

        await _req.NotifyToCallRequest("ProcessRecord", ProcessRecordInfo);
    }

    public async Task RecieveGenerateScheduleMessage() {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "GenerateSchedule", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        Dictionary<string, object> GenerateSchedulesInfo = new Dictionary<string, object>();

        consumer.Received += (model, ea) => {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(message)!;

            GenerateSchedulesInfo = data;
            
            channel.BasicAck(ea.DeliveryTag, false);
        };

        channel.BasicConsume(queue: "GenerateSchedule", autoAck: false, consumer: consumer);

        await _req.NotifyToCallRequest("GenerateSchedule", GenerateSchedulesInfo);
    }

    public async Task RecieveSubmittedScheduleMessage() {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "SubmittedSchedule", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        Dictionary<string, object> GenerateSchedulesInfo = new Dictionary<string, object>();

        consumer.Received += (model, ea) => {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(message)!;

            GenerateSchedulesInfo = data;
            
            channel.BasicAck(ea.DeliveryTag, false);
        };

        channel.BasicConsume(queue: "SubmittedSchedule", autoAck: false, consumer: consumer);

        await _req.NotifyToCallRequest("SubmittedSchedule", GenerateSchedulesInfo);
    }

    public async Task RecieveProcessedScheduleMessage() {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "ProcessedSchedule", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        Dictionary<string, object> GenerateSchedulesInfo = new Dictionary<string, object>();

        consumer.Received += (model, ea) => {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(message)!;

            GenerateSchedulesInfo = data;
            
            channel.BasicAck(ea.DeliveryTag, false);
        };

        channel.BasicConsume(queue: "ProcessedSchedule", autoAck: false, consumer: consumer);

        await _req.NotifyToCallRequest("ProcessedSchedule", GenerateSchedulesInfo);
    }

    public async Task RecieveProcessUserSubmittedWholeModeMessage() {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "ProcessUserSubmittedWholeMode", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        Dictionary<string, object> GenerateSchedulesInfo = new Dictionary<string, object>();

        consumer.Received += (model, ea) => {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(message)!;

            GenerateSchedulesInfo = data;
            
            channel.BasicAck(ea.DeliveryTag, false);
        };

        channel.BasicConsume(queue: "ProcessUserSubmittedWholeMode", autoAck: false, consumer: consumer);

        await _req.NotifyToCallRequest("ProcessUserSubmittedWholeMode", GenerateSchedulesInfo);
    }

    public async Task RecieveCreateWholeModeOrderingMessage() {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "ProcessCreateWholeModeOrdering", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        Dictionary<string, object> GenerateSchedulesInfo = new Dictionary<string, object>();

        consumer.Received += (model, ea) => {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(message)!;

            GenerateSchedulesInfo = data;
            
            channel.BasicAck(ea.DeliveryTag, false);
        };

        channel.BasicConsume(queue: "ProcessCreateWholeModeOrdering", autoAck: false, consumer: consumer);

        await _req.NotifyToCallRequest("ProcessCreateWholeModeOrdering", GenerateSchedulesInfo);
    }

    public async Task RecieveVerifyHeadMatchupsMessage() {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "ProcessVerifyHeadMatchups", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        Dictionary<string, object> GenerateSchedulesInfo = new Dictionary<string, object>();

        consumer.Received += (model, ea) => {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(message)!;

            GenerateSchedulesInfo = data;
            
            channel.BasicAck(ea.DeliveryTag, false);
        };

        channel.BasicConsume(queue: "ProcessVerifyHeadMatchups", autoAck: false, consumer: consumer);

        await _req.NotifyToCallRequest("ProcessVerifyHeadMatchups", GenerateSchedulesInfo);
    }

    public async Task RecieveRandomSelectionWholePlayoffsMessage() {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "ProcessRandomSelectionWholePlayoffs", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        Dictionary<string, object> GenerateSchedulesInfo = new Dictionary<string, object>();

        consumer.Received += (model, ea) => {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(message)!;

            GenerateSchedulesInfo = data;
            
            channel.BasicAck(ea.DeliveryTag, false);
        };

        channel.BasicConsume(queue: "ProcessRandomSelectionWholePlayoffs", autoAck: false, consumer: consumer);

        await _req.NotifyToCallRequest("ProcessRandomSelectionWholePlayoffs", GenerateSchedulesInfo);
    }

     public async Task RecieveConstructWholeBracketMessage() {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "ProcessConstructWholeBracket", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        Dictionary<string, object> GenerateSchedulesInfo = new Dictionary<string, object>();

        consumer.Received += (model, ea) => {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(message)!;

            GenerateSchedulesInfo = data;
            
            channel.BasicAck(ea.DeliveryTag, false);
        };

        channel.BasicConsume(queue: "ProcessConstructWholeBracket", autoAck: false, consumer: consumer);

        await _req.NotifyToCallRequest("ProcessConstructWholeBracket", GenerateSchedulesInfo);
    }

    public async Task RecieveUserSubmittedDivisionTypeScheduleMessage() {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "ProcessUserSubmittedDivisionTypeSchedule", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        Dictionary<string, object> GenerateSchedulesInfo = new Dictionary<string, object>();

        consumer.Received += (model, ea) => {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(message)!;

            GenerateSchedulesInfo = data;
            
            channel.BasicAck(ea.DeliveryTag, false);
        };

        channel.BasicConsume(queue: "ProcessUserSubmittedDivisionTypeSchedule", autoAck: false, consumer: consumer);

        await _req.NotifyToCallRequest("ProcessUserSubmittedDivisionTypeSchedule", GenerateSchedulesInfo);
    }
}