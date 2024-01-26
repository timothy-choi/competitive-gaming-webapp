namespace TwitchWebSocket;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.SignalR;
using System.Net.Http;
using Newtonsoft.Json;

public class NotificationHub : Hub {
    public async Task SendMessage(string msgClass, string msg) {
        await Clients.All.SendAsync(msgClass, msg);
    }
}

public class LiveStream : WebSocketBehavior {
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly HttpClient _client;

    public LiveStream(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
        _client = new HttpClient();
    }
    protected async override void OnMessage(MessageEventArgs e) {
        var liveStreamRes = JObject.Parse(e.Data);
        if (liveStreamRes["metadata"]["message_type"].Equals("session_welcome")) {
            Dictionary<String, String> reqBody = new Dictionary<String, String>();
            reqBody.Add("type", "streams.online");
            reqBody.Add("version", "1");
            Dictionary<String, String> empty = new Dictionary<String, String>();
            reqBody.Add("condition", JsonConvert.SerializeObject(empty));
            Dictionary<String, String> transport = new Dictionary<String, String>();
            transport.Add("method", "websocket");
            transport.Add("session_id", liveStreamRes["payload"]["session"]["id"].ToString());
            reqBody.Add("transport", JsonConvert.SerializeObject(transport));
            await _client.PostAsync("https://api.twitch.tv/helix/eventsub/subscriptions", new FormUrlEncodedContent(reqBody));

            reqBody = new Dictionary<String, String>();
            reqBody.Add("type", "streams.offline");
            reqBody.Add("version", "1");
            empty = new Dictionary<String, String>();
            reqBody.Add("condition", JsonConvert.SerializeObject(empty));
            transport = new Dictionary<String, String>();
            transport.Add("method", "websocket");
            transport.Add("session_id", liveStreamRes["payload"]["session"]["id"].ToString());
            reqBody.Add("transport", JsonConvert.SerializeObject(transport));
            await _client.PostAsync("https://api.twitch.tv/helix/eventsub/subscriptions", new FormUrlEncodedContent(reqBody));
        }
        var notification = liveStreamRes["payload"]["subscription"];
        string info = notification["id"]?.ToString() + " by " + notification["broadcaster_user_name"]?.ToString(); 
        if (notification != null) {
            if (notification["type"]!.Equals("streams.online")) {
                await _hubContext.Clients.All.SendAsync("streams", "Stream {info} has started!");
            }
            if (notification["type"]!.Equals("streams.offline")) {
                await _hubContext.Clients.All.SendAsync("streams", "Stream {info} has ended!");
            }
        }
    }
}


public class TwitchSocket : BackgroundService {
    private WebSocketServer ws;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<NotificationHub> _hubContext;

    public TwitchSocket(IHubContext<NotificationHub> hubContext, IConfiguration configuration) {
        _hubContext = hubContext;
        _configuration = configuration;
        ws = new WebSocketServer("wss://eventsub.wss.twitch.tv/ws");
        ws.AddWebSocketService<LiveStream>("/LiveStream", () => new LiveStream(_hubContext));
        
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        ws.Start();
        while (!stoppingToken.IsCancellationRequested) {
            if (_configuration["WebSocketStatus"] == "Stop") {
                ws.Stop();
                break;
            }
        }

        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
    }
}

