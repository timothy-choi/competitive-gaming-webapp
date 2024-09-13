namespace CompetitiveGamingApp.Models;

using CompetitiveGamingApp.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


public class LeaguePlayerSeasonAssignments {
    [BsonId]
    public String? AssignmentsId {get; set;}
    public String? ConfigId {get; set;}
    public String? LeagueId {get; set;}

    //player assignments
    public bool PartitionsEnabled {get; set;} //false = no divisons (Ex. No NFC East, MLB Central, PAC-12, etc)
    public bool ReassignEverySeason {get; set;} //true = reassign each player to different partitions at the end of each season
    public bool AutomaticInduction {get; set;} //true = enable algorithm to automatically assign a new player / or exisiting player into a new partition, false = owner manually adds player into a division
    public int NumberOfPlayersPerPartition {get; set;} //min # of players in each partition
    public int NumberOfPartitions {get; set;} //# of partitions in the league 
    //season matchups
    public bool AutomaticScheduling {get; set;} //true = algorithm determines schedule, false = owner manually enters a schedule via json for verification
    public bool ExcludeOutsideGames {get; set;} //true = players can only play games in their partition, false = players can play games with players in other parititons or partitions are not enabled
    public int InterDvisionGameLimit {get; set;} //# of games played against players outside of player's division
    public bool RepeatMatchups {get; set;} //true = players can play other players in different partitions > 1 in the season 
    public int MaxRepeatMatchups {get; set;} //# of times players can play with repeat players in a different partition in a season at the Max
    public bool DivisionSelective {get; set;} //true = if outside games (out of division) are enabled, then only teams in select divisions will play the said team
    public Dictionary<string, List<string>> OutsideDivisionSelections {get; set;} //List of all division that a teams can play for outside-division games
    public bool RandomizeDivisionSelections {get; set;}
    public bool PlayerSelection {get; set;} //true = if league has no divisions, can be selective in which players to play (won't be able to play any other player)
    public Dictionary<string, List<string>> PlayerExemptLists {get; set;} //list of all players you do not want to play for each player
    public bool repeatAllMatchups {get; set;} //true = if league has no divisions, players need to play each other at least twice.
    public int minRepeatMatchups {get; set;} //# of repeat matchup each player needs to face at a minimum
    public int maxRepeatMatchups {get; set;}
    public bool playAllPlayers {get; set;} //true = all players need to play one another at least once

    
    public Dictionary<String, List<String>>? AllPartitions {get; set;}
    public Dictionary<String, List<String>>? AllCombinedDivisions {get; set;}
    public List<Tuple<string, List<object>>>? PlayerFullSchedule {get; set;}
    public List<List<Tuple<string, List<object>>>>? ArchievePlayerFullSchedule {get; set;}
    public List<SingleGame>? FinalFullSchedule {get; set;}
    public List<List<SingleGame>>? ArchieveFinalFullSchedule {get; set;}
    











}