namespace TwitchWebSocket;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.SignalR;


public class LiveStream : WebSocketBehavior {
    protected override void OnMessage(MessageEventArgs e) {
        var liveStreamRes = JObject.Parse(e.Data);
        

    }
}


public class TwitchSocket {
    private WebSocketServer ws;
    public TwitchSocket() {
        ws = new WebSocketServer("wss://eventsub.wss.twitch.tv/ws");
        ws.AddWebSocketService<LiveStream>("/LiveStream");
        ws.Start();
        
        ws.Stop();
    }
}

