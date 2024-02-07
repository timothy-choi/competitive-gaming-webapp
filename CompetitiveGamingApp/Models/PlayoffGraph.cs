namespace CompetitiveGamingApp.Models;



public class PlayoffGraphNode {
    public PlayoffMatchup currentPlayoffMatchup {get; set;}
    public PlayoffGraphNode? NextPlayoffMatch {get; set;} 
    public PlayoffGraphNode(PlayoffMatchup playoffMatchup) {
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
    public void AddHeadMatchup(PlayoffMatchup playoffMatchup) {
        PlayoffGraphNode head = new PlayoffGraphNode(playoffMatchup);
        PlayoffHeadMatchups?.Add(head);
    }

    public PlayoffGraphNode GetFinalGraphNode() {
        if (PlayoffHeadMatchups?.Count < 1) {
            throw new Exception("Invalid # of teams");
        }
        if (PlayoffHeadMatchups?.Count == 1) {
            return PlayoffHeadMatchups[0]!;
        }
        PlayoffGraphNode curr = PlayoffHeadMatchups?[0]!;
        while (curr.NextPlayoffMatch != null) {
            curr = curr.NextPlayoffMatch;
        }
        return curr;
    }
    public void ConnectHeadToNext(PlayoffGraphNode nextMatchup, PlayoffGraphNode prevMatchup) {
        prevMatchup.NextPlayoffMatch = nextMatchup;
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
    public void AddSubPlayoffBracket(String name) {
        PlayoffGraph subBracket = new PlayoffGraph(name);
        SubPlayoffBrackets?.Add(subBracket);
    }

    public void addFinalRoundMatchup(string winner1, string winner2) {
        PlayoffMatchup finalRoundMatchup = new PlayoffMatchup();
        finalRoundMatchup.player1 = winner1;
        finalRoundMatchup.player2 = winner2;

        FinalRoundMatchups?.Add(new PlayoffGraphNode(finalRoundMatchup));
    }

    public PlayoffGraphNode AddRemainingGames(List<PlayoffGraphNode?>? AllNodes) {
        if (FinalRoundMatchups?.Count == 1) {
            return AllNodes![0]!;
        }

        double sqrt = Math.Sqrt(AllNodes!.Count);

        if (AllNodes.Count % 2 != 0 || (AllNodes.Count % 2 == 0 && Math.Sqrt(AllNodes.Count) == (int) sqrt)) {
            throw new Exception("Can't set up final rounds of playoffs");
        }

        PlayoffGraphNode one = AddRemainingGames(AllNodes.GetRange(0, AllNodes!.Count / 2));
        PlayoffGraphNode two = AddRemainingGames(AllNodes.GetRange(AllNodes!.Count / 2, AllNodes!.Count - (AllNodes!.Count / 2)));

        PlayoffMatchup championshipGame = new PlayoffMatchup();
        PlayoffGraphNode championshipNode = new PlayoffGraphNode(championshipGame);

        if (one != null && two != null)
        {
            championshipNode.NextPlayoffMatch = one;
            one.NextPlayoffMatch = two;
        }
        else if (one != null)
        {
            championshipNode.NextPlayoffMatch = one;
        }
        else if (two != null)
        {
            championshipNode.NextPlayoffMatch = two;
        }

        return championshipNode;
    }

    public void SetChampion(string playerName) {
        Champion = playerName;
    }
}