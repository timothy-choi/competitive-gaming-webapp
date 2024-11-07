namespace CompetitiveGamingApp.Models;




public class InGameScore
{
    public int Desc { get; set; }
    public int ScoreHost { get; set; }
    public int ScoreGuest { get; set; }
}


public class SingleGame {
    public String? SingleGameId {get; set;}
    public String? hostPlayer {get; set;}
    public String? guestPlayer {get; set;}
    public int hostScore {get; set;}

    public int guestScore {get; set;}
    public TimeSpan timePlayed {get; set;}
    public String? gameEditor {get; set;}
    public String? twitchBroadcasterId {get; set;}
    public Dictionary<String, String>? otherGameInfo {get; set;}
    public String? predictionId {get; set;}
    public String? videoFilePath {get; set;}
}