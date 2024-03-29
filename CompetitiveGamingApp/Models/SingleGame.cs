namespace CompetitiveGamingApp.Models;

public class SingleGame {
    public String? SingleGameId {get; set;}
    public String? hostPlayer {get; set;}
    public String? guestPlayer {get; set;}
    public Tuple<int, int>? finalScore {get; set;}
    public List<Tuple<String, Tuple<int, int>>>? inGameScores {get; set;}
    public DateTime timePlayed {get; set;}
    public String? gameEditor {get; set;}
    public String? twitchBroadcasterId {get; set;}
    public Dictionary<String, String>? otherGameInfo {get; set;}
    public String? predictionId {get; set;}
    public String? videoFilePath {get; set;}
}