namespace CompetitiveGamingApp.Models;


public class League {
    public String? LeagueId {get; set;}
    public String? Name {get; set;}
    public String? Owner {get; set;}
    public String? Description {get; set;}
    public List<Dictionary<String, Object?>>? Players {get; set;}
    public List<String?>? tags {get; set;}
    public String? LeagueConfig {get; set;}
    public String? SeasonAssignments {get; set;}
    public List<Dictionary<String, String>>? LeagueStandings {get; set;}
}