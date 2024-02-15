namespace CompetitiveGamingApp.Models;

public class PlayerInfo {
    public String? Id {get; set;}
    public String? Name {get; set;}
    public String? Username {get; set;}
    public String? PlayerId {get; set;}
}

public class GameInfo {
    public String? Id {get; set;}
    public String? Matchup {get; set;}
    public String? GameId {get; set;}
}

public class LeagueInfo {
    public String? Id {get; set;}
    public String? LeagueName {get; set;}
    public String? LeagueId {get; set;}
}