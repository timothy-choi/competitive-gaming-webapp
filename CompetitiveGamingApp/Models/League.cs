namespace CompetitiveGamingApp.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


public class LeagueStandings {
    public String? LeagueStandingsId {get; set;}
    public int Season {get; set;}
    public List<Dictionary<String, object>>? Table {get; set;}
}

public class DivisionStandings {
    public String? DivisionStandingsId {get; set;}
    public int Season {get; set;}
    public String? DivisionName {get; set;}
    public List<Dictionary<String, object>>? Table {get; set;}
}

public class CombinedDivisionStandings {
    public String? CombinedDivisionStandingsId {get; set;}
    public int Season {get; set;}
    public String? CombinedDivisionName {get; set;}
    public List<string> Divisions {get; set;}
    public List<Dictionary<String, object>>? Table {get; set;}
}



public class League {
    [BsonId]
    public String? LeagueId {get; set;}
    public String? Name {get; set;}
    public String? Owner {get; set;}
    public String? Description {get; set;}
    public List<Dictionary<String, Object?>>? Players {get; set;}
    public List<String?>? tags {get; set;}
    public String? LeagueConfig {get; set;}
    public String? SeasonAssignments {get; set;}
    public LeagueStandings? LeagueStandings {get; set;}
    public List<LeagueStandings>? AchieveLeagueStandings {get; set;}
    public Dictionary<String, DivisionStandings>? DivisionStandings {get; set;}
    public List<Dictionary<String, DivisionStandings>>? ArchieveDivisionStandings {get; set;}
    public Dictionary<String, CombinedDivisionStandings>? CombinedDivisionStandings {get; set;} //Combined Division = Conferences in NFL, NBA, MLB, etc.
    public List<Dictionary<String, CombinedDivisionStandings>>? ArchieveCombinedDivisionStandings {get; set;}
    public List<Tuple<String, String>>? Champions {get; set;} 
    public String? PlayoffAssignments {get; set;}
}