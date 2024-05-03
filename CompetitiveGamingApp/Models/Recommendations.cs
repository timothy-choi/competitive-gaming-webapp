namespace CompetitiveGamingApp.Models;


public class PlayerGameRecord {
    public String? PlayerGameRecordId {get; set;}
    public String? PlayerId {get; set;}
    public String? PlayerUsername {get; set;}
    public List<int>? PlayerRecord {get; set;}
    public bool PlayerLeagueJoined {get; set;}
    public String? PlayerLeague {get; set;}
    public List<String?>? PlayerLeagueTags {get; set;}
}

public class PlayerRecommendations {
    public String? PlayerRecommendationId {get; set;}
    public String? PlayerId {get; set;}
    public String? PlayerUsername {get; set;}
    public List<PlayerGameRecord?>? PlayerHistoryRecords {get; set;}
}

public class LeagueJoinRecord {
    public String? LeagueJoinRecordId {get; set;}
    public String? LeagueId {get; set;}
    public String? LeagueName {get; set;}
    public List<String?>? LeagueTags {get; set;}
    public List<int>? LeaguePlayerOverallRecord {get; set;}
    public List<List<int>>? LeagueIndividualOverallRecord {get; set;}
}

public class LeagueRecommendations {
    public String? LeagueRecommendationId {get; set;}
    public String? PlayerId {get; set;}
    public String? PlayerUsername {get; set;}
    public List<LeagueJoinRecord?>? LeagueJoinedHistoryRecord {get; set;}
}

