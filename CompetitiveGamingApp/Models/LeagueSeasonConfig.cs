namespace CompetitiveGamingApp.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
public class LeagueSeasonConfig {
    [BsonId]
    public String? ConfigId {get; set;}
    public String? LeagueName {get; set;}

    public int commitmentLength {get; set;} //how long players will need to stay in a league before leaving. If seasons is on, # of season. Otherwise, time (in months)
    public int feePrice {get; set;} //cost to join a league
    public int NumberOfPlayersLimit {get; set;}  //# of players allowed in this league. 
    public int NumberOfPlayersMin {get; set;} //# of players that needs to be in a league for a season to start (if enabled)
    public bool JoinDuringSeason {get; set;} // true = can accept new players to join league after season has started, false = deny new members (need to wait during offseason)
    public bool convertToRegular {get; set;} //true = automatically convert seasons mode to regular mode (no playoffs)


    public bool seasons {get; set;} //true = seasons mode, false = continuous play (uncompetitive)
    public int NumberOfGames {get; set;} //Number of Games played in a season, will be 0 if seasons is marked false
    public bool selfScheduleGames {get; set;} //true = owner will organize matchs, false = automatic scheduling
    public int intervalBetweenGames {get; set;} //# of days off between matches if selfScheduleGames is set to false
    public int intervalBetweenGamesHours {get; set;} //# of hours off between matchs if selfScheduleGames is set to false. If want < 1 day off between playing next match then set this to non zero val and intervalsBetweenGames to 0. 
    public List<Tuple<string, DateTime>> firstSeasonMatch {get; set;} //DateTime of date of first game of a season (only on if selfScheduleGames is false)
    public bool tiesAllowed {get; set;} //true = game can end in tie


    public List<Tuple<string, DateTime>> playoffStart {get; set;} //DateTime of date of first game of the playoffs of a season (only on if selfScheduleGames is false)
    public int intervalBetweenPlayoffRoundGames {get; set;} //# of days off between playoff matches if selfScheduleGames is set to false
    public int intervalBetweenPlayoffRoundGamesHours {get; set;} //# of hours off between playoff matchs if selfScheduleGames is set to false. If want < 1 day off between playing next match then set this to non zero val and intervalsBetweenGames to 0. 
    public int intervalBetweenRounds {get; set;} // # of days before next round 
    public int intervalBetweenRoundsHours {get; set;} // Same idea as intervalBetweenRounds but in hours where either you want to start a new round on same day as previous round or add on intervalBeetweenRounds
    public bool playoffContention {get; set;} //true = there will be a playoffs for each season
    public bool playoffEligibleLimit {get; set;} //true =  not all players will be allowed to play in playoffs
    public int PlayoffSizeLimit {get; set;} //how many players will be allowed to play in playoffs
    public bool PlayoffSeries {get; set;} //true = each round requires players to win a best of n series, false = one game
    public int SeriesLengthMax {get; set;} // # of games that a player needs to win the series
    public bool sameSeriesLength {get; set;} // true = all rounds need to a series where winner wins best of n games each, false = differeing series lengths or each round is 1 game only
    public List<int> GamesPerRound {get; set;}
    public bool BreakTiesViaGame {get; set;} //true = add extra regular season game if ties are allowed to break ties, false = don't but rely on other statistics to break ties or rely on position in each partition
    public List<String>? otherMetrics {get; set;} // other metrics to compare other players if BreakTiesViaGame is false
}