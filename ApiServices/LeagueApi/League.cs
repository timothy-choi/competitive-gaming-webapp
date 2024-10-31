namespace CompetitiveGamingApp.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


public class LeagueTable {
    public String? LeagueTableId {get; set;}
    public int Season {get; set;}
    public List<Dictionary<String, object>>? Table {get; set;}
}

public class DivisionTable {
    public String? DivisionTableId {get; set;}
    public int Season {get; set;}
    public String? DivisionName {get; set;}
    public List<Dictionary<String, object>>? Table {get; set;}
}

public class CombinedDivisionTable {
    public String? CombinedDivisionTableId {get; set;}
    public int Season {get; set;}
    public String? CombinedDivisionName {get; set;}
    public List<string> Divisions {get; set;}
    public List<Dictionary<String, object>>? Table {get; set;}
}



public class League {
    [BsonId]
    public ObjectId Id { get; set; } 
    public String? LeagueId {get; set;}
    public String? Name {get; set;}
    public String? Owner {get; set;}
    public String? Description {get; set;}
    public List<Dictionary<String, Object?>>? Players {get; set;}
    public List<String?>? tags {get; set;}
    public String? LeagueConfig {get; set;}
    public String? SeasonAssignments {get; set;}
    public LeagueTable? LeagueStandings {get; set;}
    public List<LeagueTable>? AchieveLeagueStandings {get; set;}
    public Dictionary<String, DivisionTable>? DivisionStandings {get; set;}
    public List<Dictionary<String, DivisionTable>>? ArchieveDivisionStandings {get; set;}
    public Dictionary<String, CombinedDivisionTable>? CombinedDivisionStandings {get; set;} //Combined Division = Conferences in NFL, NBA, MLB, etc.
    public List<Dictionary<String, CombinedDivisionTable>>? ArchieveCombinedDivisionStandings {get; set;}
    public List<Tuple<String, String>>? Champions {get; set;} 
    public String? PlayoffAssignments {get; set;}
    public int? Season {get; set;}
    public DateTime StartDate {get; set;}
}