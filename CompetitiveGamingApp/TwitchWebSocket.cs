namespace TwitchWebSocket;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.SignalR;

public class NotificationHub : Hub {
    public async Task SendMessage(string msgClass, string msg) {
        await Clients.All.SendAsync(msgClass, msg);
    }
}

public class LiveStream : WebSocketBehavior {
    private readonly IHubContext<NotificationHub> _hubContext;

    public LiveStream(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    protected async override void OnMessage(MessageEventArgs e) {
        var liveStreamRes = JObject.Parse(e.Data);
        
        var notification = liveStreamRes["payload"]["subscription"];
        if (notification != null) {
            if (notification["type"].Equals("streams.online")) {
                await _hubContext.Clients.All.SendAsync("streams", "Stream has started!");
            }
            if (notification["type"].Equals("streams.offline")) {
                await _hubContext.Clients.All.SendAsync("streams", "Stream has ended!");
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

