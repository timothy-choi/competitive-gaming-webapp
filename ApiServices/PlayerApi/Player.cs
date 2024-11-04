namespace CompetitiveGamingApp.Models;



public class Player {
    public string? playerId { get; set; }
    public string? playerName { get; set; }
    public string? playerUsername { get; set; }
    public string? playerEmail { get; set; }
    public DateTime playerJoined { get; set; }
    public bool playerAvailable { get; set; }
    public bool playerInGame { get; set; }
    public List<string>? playerFriends { get; set; }
    public bool leagueJoined { get; set; }
    public string? playerLeagueJoined { get; set; }
    
    // Update singlePlayerRecord to byte[] to match varbinary(max) in SQL Server
    public byte[]? singlePlayerRecord { get; set; }
    
    public double singleGamePrice { get; set; }
    public bool enablePushNotifications { get; set; }
}
