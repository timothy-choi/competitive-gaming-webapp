namespace CompetitiveGamingApp.Models;



public class Player {
    public String? playerId {get; set;}
    public String? playerName {get; set;}
    public String? playerUsername {get; set;}
    public String? playerEmail {get; set;}
    public DateTime playerJoined {get; set;}
    public bool playerAvailable {get; set;}
    public bool playerInGame {get; set;}
    public List<String>? playerFriends {get; set;}
    public bool leagueJoined {get; set;}
    public String? playerLeagueJoined {get; set;}
    public List<int>? singlePlayerRecord {get; set;}
    public int playerScore {get; set;}
}