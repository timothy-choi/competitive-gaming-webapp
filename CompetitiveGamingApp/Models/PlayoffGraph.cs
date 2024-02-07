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
    List<PlayoffGraphNode?>? PlayoffHeadMatchups {get; set;}

    public PlayoffGraph() {
        PlayoffHeadMatchups = new List<PlayoffGraphNode>();
    }
}