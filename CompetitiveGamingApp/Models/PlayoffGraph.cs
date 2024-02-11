namespace CompetitiveGamingApp.Models;



public class PlayoffGraphNode {
    public PlayoffMatchup currentPlayoffMatchup {get; set;}
    public PlayoffGraphNode? NextPlayoffMatch {get; set;} 
    public PlayoffGraphNode? PrevPlayoffRoundMatch {get; set;}
    public PlayoffGraphNode(PlayoffMatchup playoffMatchup) {
        currentPlayoffMatchup = playoffMatchup;
        NextPlayoffMatch = null;
    }
}

public class PlayoffGraph {
    public List<PlayoffGraphNode> PlayoffHeadMatchups {get; set;}
    public List<Tuple<int, PlayoffGraphNode>> AllOtherMatchups {get; set;}
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

    public PlayoffGraphNode FindPlayerMatchup(string player1, string player2) {
        PlayoffGraphNode? node = null;

        foreach (var startNode in PlayoffHeadMatchups!) {
            while (startNode != null) {
                if ((startNode.currentPlayoffMatchup.player1 == player1 && startNode.currentPlayoffMatchup.player2 == player2) || (startNode.currentPlayoffMatchup.player1 == player2 && startNode.currentPlayoffMatchup.player2 == player1)) {
                    node = startNode;
                    break;
                }
            }
        }

        if (node != null) {
            return node;
        }

        foreach (var otherNode in AllOtherMatchups) {
            while (otherNode.Item2 != null) {
                if ((otherNode.Item2.currentPlayoffMatchup.player1 == player1 && otherNode.Item2.currentPlayoffMatchup.player2 == player2) || (otherNode.Item2.currentPlayoffMatchup.player1 == player2 && otherNode.Item2.currentPlayoffMatchup.player2 == player1)) {
                    node = otherNode.Item2;
                    break;
                }
            }
        }

        if (node == null) {
            throw new Exception("Couldn't find playoff matchup");
        }

        return node!;
    }

    public PlayoffGraphNode FindByPosition(int round, int matchup) {
        if (round == 1) {
            return PlayoffHeadMatchups[matchup];
        }
        var round_matchups = AllOtherMatchups.GetRange(AllOtherMatchups.IndexOf(AllOtherMatchups.FirstOrDefault(t => t.Item1 == round)!), AllOtherMatchups.Count(tuple => tuple.Item1 == round));
        return round_matchups[matchup].Item2;
    }

    private void ConnectHeadToNext(PlayoffGraphNode nextMatchup, PlayoffGraphNode prevMatchup) {
        prevMatchup.NextPlayoffMatch = nextMatchup;
    }

    private void ConnectCurrToPrev(PlayoffGraphNode prevMatchup, PlayoffGraphNode currMatchup) {
        currMatchup.PrevPlayoffRoundMatch = prevMatchup;
    }

    public List<Tuple<PlayoffGraphNode, PlayoffGraphNode>> ConnectRounds(List<Tuple<PlayoffGraphNode, PlayoffGraphNode>> beginningNodes) {
        List<Tuple<PlayoffGraphNode, PlayoffGraphNode>> res = new List<Tuple<PlayoffGraphNode, PlayoffGraphNode>>();
        for (int i = 0; i < beginningNodes.Count; ++i) {
            ConnectHeadToNext(beginningNodes[i].Item2, beginningNodes[i].Item1);
            ConnectCurrToPrev(beginningNodes[i].Item1, beginningNodes[i].Item2);
            res.Add(Tuple.Create(beginningNodes[i].Item1, beginningNodes[i].Item2));
        }
        return res;
    }
}

public class PlayoffBracket {
    public List<PlayoffGraph> SubPlayoffBrackets {get; set;}
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

    public void addFinalRoundMatchup(string winner1, int rank1, string winner2, int rank2) {
        PlayoffMatchup finalRoundMatchup = new PlayoffMatchup();
        finalRoundMatchup.PlayoffMatchupId = Guid.NewGuid().ToString();
        finalRoundMatchup.player1 = winner1;
        finalRoundMatchup.player1_rank = rank1;
        finalRoundMatchup.player2 = winner2;
        finalRoundMatchup.player2_rank = rank2;

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

    public PlayoffGraphNode FindFinalRoundMatchup(string player1, string player2) {
         PlayoffGraphNode? node = null;

        foreach (var startNode in FinalRoundMatchups!) {
            while (startNode != null) {
                if ((startNode.currentPlayoffMatchup.player1 == player1 && startNode.currentPlayoffMatchup.player2 == player2) || (startNode.currentPlayoffMatchup.player1 == player2 && startNode.currentPlayoffMatchup.player2 == player1)) {
                    node = startNode;
                    break;
                }
            }
        }

        if (node == null) {
            throw new Exception("Couldn't find playoff matchup");
        }

        return node!;
    }

    public void SetChampion(string playerName) {
        Champion = playerName;
    }
}