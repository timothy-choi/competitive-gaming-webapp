namespace CompetitiveGamingApp.Models;



public class PlayoffGraphNode {
    public PlayoffMatchup currentPlayoffMatchup {get; set;}
    public PlayoffGraphNode? NextPlayoffMatch {get; set;} 
    public PlayoffGraphNode(PlayoffMatchup playoffMatchup, bool GraphHead) {
        currentPlayoffMatchup = playoffMatchup;
        NextPlayoffMatch = null;
    }
}

public class PlayoffGraph {
    public List<PlayoffGraphNode?>? PlayoffHeadMatchups {get; set;}
    public String? PlayoffName {get; set;}

    public PlayoffGraph(String name) {
        PlayoffHeadMatchups = new List<PlayoffGraphNode?>();
        PlayoffName = name;
    }
}

public class PlayoffBracket {
    public List<PlayoffGraph?>? SubPlayoffBrackets {get; set;}
    public List<PlayoffGraphNode?>? FinalRoundMatchups {get; set;}
    public String? Champion {get; set;}

    public PlayoffBracket() {
        SubPlayoffBrackets = new List<PlayoffGraph?>();
        FinalRoundMatchups = new List<PlayoffGraphNode?>();
        Champion = "";
    }
}