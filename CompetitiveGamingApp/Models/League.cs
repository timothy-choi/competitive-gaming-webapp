namespace CompetitiveGamingApp.Models;


public class League {
    public String? LeagueId {get; set;}
    public String? Name {get; set;}
    public String? Owner {get; set;}
    public List<String?>? Players {get; set;}
    public List<String?>? tags {get; set;}
    public String? LeagueConfig {get; set;}
}