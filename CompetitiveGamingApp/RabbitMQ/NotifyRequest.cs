using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace CompetitiveGamingApp.RabbitMQ;

public class NotifyRequest : Hub {
    public NotifyRequest() {}
    public async Task NotifyToCallRequest(string topic, Dictionary<string, object> bodyData) {
        await Clients.All.SendAsync(topic, JsonConvert.SerializeObject(bodyData));
    }
}