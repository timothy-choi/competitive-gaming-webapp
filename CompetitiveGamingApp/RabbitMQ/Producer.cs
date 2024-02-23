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

    public void SendVerifyHeadMatchupsMessage<T>(T message) {
        var connection = _factory.CreateConnection();

        using var ProcessVerifyHeadMatchupsChannel = connection.CreateModel();

        ProcessVerifyHeadMatchupsChannel.QueueDeclare("ProcessVerifyHeadMatchups", exclusive: false);

        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        ProcessVerifyHeadMatchupsChannel.BasicPublish(exchange: "", routingKey: "ProcessVerifyHeadMatchups", body: data);
    }

    public void SendRandomSelectionWholePlayoffsMessage<T>(T message) {
        var connection = _factory.CreateConnection();

        using var ProcessRandomSelectionWholePlayoffsChannel = connection.CreateModel();

        ProcessRandomSelectionWholePlayoffsChannel.QueueDeclare("ProcessRandomSelectionWholePlayoffs", exclusive: false);

        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        ProcessRandomSelectionWholePlayoffsChannel.BasicPublish(exchange: "", routingKey: "ProcessRandomSelectionWholePlayoffs", body: data);
    }

    public void SendConstructWholeBracketMessage<T>(T message) {
        var connection = _factory.CreateConnection();

        using var ProcessConstructWholeBracketChannel = connection.CreateModel();

        ProcessConstructWholeBracketChannel.QueueDeclare("ProcessConstructWholeBracket", exclusive: false);

        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        ProcessConstructWholeBracketChannel.BasicPublish(exchange: "", routingKey: "ProcessConstructWholeBracket", body: data);
    }

    public void SendUserSubmittedDivisionTypeScheduleMessage<T>(T message) {
        var connection = _factory.CreateConnection();

        using var ProcessUserSubmittedDivisionTypeScheduleChannel = connection.CreateModel();

        ProcessUserSubmittedDivisionTypeScheduleChannel.QueueDeclare("ProcessUserSubmittedDivisionTypeSchedule", exclusive: false);

        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        ProcessUserSubmittedDivisionTypeScheduleChannel.BasicPublish(exchange: "", routingKey: "ProcessUserSubmittedDivisionTypeSchedule", body: data);
    }

    public void SendCreateDivisionBasedPlayoffModeFormatMessage<T>(T message) {
         var connection = _factory.CreateConnection();

        using var ProcessUserSubmittedDivisionTypeScheduleChannel = connection.CreateModel();

        ProcessUserSubmittedDivisionTypeScheduleChannel.QueueDeclare("ProcessCreateDivisionBasedPlayoffModeFormat", exclusive: false);

        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        ProcessUserSubmittedDivisionTypeScheduleChannel.BasicPublish(exchange: "", routingKey: "ProcessCreateDivisionBasedPlayoffModeFormat", body: data);
    }

    public void SendRandomDivisionBasedBracketMessage<T>(T message) {
        var connection = _factory.CreateConnection();

        using var ProcessRandomDivisionBasedBracketChannel = connection.CreateModel();

        ProcessRandomDivisionBasedBracketChannel.QueueDeclare("ProcessRandomDivisionBasedBracket", exclusive: false);

        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        ProcessRandomDivisionBasedBracketChannel.BasicPublish(exchange: "", routingKey: "ProcessRandomDivisionBasedBracket", body: data);
    }

    public void SendCreateDivisionBasedBracketMessage<T>(T message) {
        var connection = _factory.CreateConnection();

        using var ProcessCreateDivisionBasedBracketChannel = connection.CreateModel();

        ProcessCreateDivisionBasedBracketChannel.QueueDeclare("ProcessCreateDivisionBasedBracket", exclusive: false);

        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        ProcessCreateDivisionBasedBracketChannel.BasicPublish(exchange: "", routingKey: "ProcessCreateDivisionBasedBracket", body: data);
    }

    public void SendSetupFinalRoundsMessage<T>(T message) {
        var connection = _factory.CreateConnection();

        using var ProcessSetupFinalRoundsChannel = connection.CreateModel();

        ProcessSetupFinalRoundsChannel.QueueDeclare("ProcessSetupFinalRounds", exclusive: false);

        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        ProcessSetupFinalRoundsChannel.BasicPublish(exchange: "", routingKey: "ProcessSetupFinalRounds", body: data);
    }

    public void SendUserDefinedPlayoffMessage<T>(T message) {
        var connection = _factory.CreateConnection();

        using var ProcessUserDefinedPlayoffFormatChannel = connection.CreateModel();

        ProcessUserDefinedPlayoffFormatChannel.QueueDeclare("ProcessUserDefinedPlayoff", exclusive: false);

        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        ProcessUserDefinedPlayoffFormatChannel.BasicPublish(exchange: "", routingKey: "ProcessUserDefinedPlayoff", body: data);
    }

    public void SendConstructUserDefinedPlayoffMessage<T>(T message) {
        var connection = _factory.CreateConnection();

        using var ProcessConstructUserDefinedPlayoffFormatChannel = connection.CreateModel();

        ProcessConstructUserDefinedPlayoffFormatChannel.QueueDeclare("ProcessConstructUserDefinedPlayoff", exclusive: false);

        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        ProcessConstructUserDefinedPlayoffFormatChannel.BasicPublish(exchange: "", routingKey: "ProcessConstructUserDefinedPlayoff", body: data);
    }
}