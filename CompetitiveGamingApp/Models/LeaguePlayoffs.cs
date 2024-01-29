namespace CompetitiveGamingApp.Models;

public class PlayoffMatchup {
    public String? player1 {get; set;}
    public int player1_rank {get; set;}
    public String? player2 {get; set;}
    public int player2_rank {get; set;}
    public int round {get; set;}
    public String? Division {get; set;}
}

public class LeaguePlayoffs {
    public string? LeaguePlayoffId {get; set;}
    public String? LeagueId {get; set;}
    public bool RandomInitialMode {get; set;}  //true = playoff selection is done randomnly, first round matchups are selected randomnly 
    public bool RandomRoundMode {get; set;} //true = after each round, playoff selection matchups are done randomly
    public bool WholeMode {get; set;} //true = playoff selection is done as a whole; all the players will be initially determined regardless of partition, false = playoff selection is initially determined based on rank within partition
    public bool DefaultMode {get; set;} //true = playoff selection is done in the default way (all teams play in the "first round"), false = special playoff format, esp in first round (Can't use this if WholeMode is false)
    
    public List<Tuple<int, Tuple<int, int>>>? WholeRoundOrdering {get; set;} //sets all matchups based on seed in the inital round (all teams initially play)
    public List<PlayoffMatchup>? WholePlayoffPairings {get; set;} //list of all pairings in whole mode
    public List<Tuple<String, List<int>>>? CombinedDivisionGroups {get; set;}
    public List<Tuple<String, List<PlayoffMatchup>>>? CombinedDivisionPlayoffMatchups {get; set;}
    public List<Tuple<String, Tuple<int, Tuple<String, String>>>>? DivisionBasedPlayoffPairings {get; set;} //division based playoff format
    public List<Tuple<String, PlayoffMatchup>>? DivisionBasedPlayoffMatchups {get; set;}
    public List<Tuple<int, Tuple<String, String>>>? UserDefinedPlayoffMatchups {get; set;} //User Defined Playoff Matchups 
    public List<PlayoffMatchup>? UserDefinedPlayoffFinalGroups {get; set;}
}