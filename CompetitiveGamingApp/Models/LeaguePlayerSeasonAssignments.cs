namespace CompetitiveGamingApp.Models;

public class LeaguePlayerSeasonAssignments {
    public String? AssignmentsId {get; set;}
    public String? ConfigId {get; set;}
    public String? LeagueId {get; set;}

    //player assignments
    public bool PartitionsEnabled {get; set;} //false = no divisons (Ex. No NFC East, MLB Central, PAC-12, etc)
    public bool ReassignEverySeason {get; set;} //true = reassign each player to different partitions at the end of each season
    public bool AutomaticInduction {get; set;} //true = enable algorithm to automatically assign a new player / or exisiting player into a new partition, false = owner manually adds player into a division
    public int NumberOfPlayersPerPartition {get; set;} //min # of players in each partition
    public int NumberOfPartitions {get; set;} //# of partitions in the league 
    public bool SamePartitionSize {get; set;} //true = all partitions need to have same # of players (Note: for this to be true, # of players in league needs to be divisible by # of partitions)

    //season matchups
    public bool AutomaticScheduling {get; set;} //true = algorithm determines schedule, false = owner manually enters a schedule via json for verification
    public bool ExcludeOutsideGames {get; set;} //true = players can only play games in their partition, false = players can play games with players in other parititons or partitions are not enabled
    public bool InterDvisionGameLimit {get; set;} //true = limit on # of games played against players outside of player's division
    public bool RepeatMatchups {get; set;} //true = players can play other players in different partitions > 1 in the season 
    public int MaxRepeatMatchups {get; set;} //# of times players can play with repeat players in a different partition in a season at the Max
    public bool DivisionSelective {get; set;} //true = if outside games (out of division) are enabled, then only teams in select divisions will play the said team
    public List<string> OutsideDivisionSelections {get; set;} //List of all division that a teams can play for outside-division games



    
    public Dictionary<String, List<String>>? AllPartitions {get; set;}
    public List<Tuple<String, String>>? FinalFullSchedule {get; set;}
    











}