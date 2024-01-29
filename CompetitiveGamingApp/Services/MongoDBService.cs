using MongoDB.Driver;
using MongoDB.Bson;
using CompetitiveGamingApp.Models;


namespace CompetitiveGamingApp.Services;


public class MongoDBService {
    private readonly MongoClient client;
    public MongoDBService(IConfiguration configuration) {
        client = new MongoClient(configuration.GetConnectionString("MongoDB_URI"));
    }
    public object GetData(String db, String entityId) {
        var db_collection = client.GetDatabase("league").GetCollection<BsonDocument>(db);
        if (db == "leagueInfo") {
            var filterById = Builders<LeagueInfo>.Filter.Eq(league => league.leagueId, entityId);
            var res = db_collection.Find(filterById).FirstOrDefault();
            return res;
        }
        if (db == "leagueConfig") {
            var filterById = Builders<LeagueConfig>.Filter.Eq(league => league.leagueId, entityId);
            var res = db_collection.Find(filterById).FirstOrDefault();
            return res;
        }
        if (db == "leagueSeasonAssignments") {
            var filterById = Builders<LeagueSeasonConfig>.Filter.Eq(league => league.leagueId, entityId);
            var res = db_collection.Find(filterById).FirstOrDefault();
            return res;
        }
        var filterById = Builders<LeaguePlayoffConfig>.Filter.Eq(league => league.leagueId, entityId);
        var res = db_collection.Find(filterById).FirstOrDefault();
        return res;
    }

    public void PostData(String db, object document) {
        object doc;
        if (db == "leagueInfo") {
            doc = new League {
                LeagueId = document.LeagueId,
                Name = document.Name,
                Owner = document.Owner,
                Description = document.Description,
                Players = document.Players,
                tags = document.tags,
                LeagueConfig = document.LeagueConfig,
                SeasonAssignments = document.SeasonAssignments,
                LeagueStandings = document.LeagueStandings,
                DivisionStandings = document.DivisionStandings,
                CombinedDivisionStandings = document.CombinedDivisionStandings,
                Champions = document.Champions,
                PlayoffAssignments = document.PlayoffAssignments
            };
        }
        if (db == "leagueConfig") {
            doc = new LeagueSeasonConfig {
                ConfigId = document.ConfigId,
                LeagueName = document.LeagueName,
                commitmentLength = document.commitmentLength,
                feePrice = document.feePrice,
                NumberOfPlayersLimit = document.NumberOfPlayersLimit,
                JoinDuringSeason = document.JoinDuringSeason,
                convertToRegular = document.convertToRegular,
                seasons = document.seasons,
                NumberOfGames = document.NumberOfGames,
                selfScheduleGames = document.selfScheduleGames,
                intervalBetweenGames = document.intervalBetweenGames,
                intervalBetweenGameHours = document.intervalBetweenGameHours,
                firstSeasonMatch = document.firstSeasonMatch,
                tiesAllowed = document.tiesAllowed,
                playoffStart = document.playoffStart,
                intervalBetweenPlayoffRoundGames = document.intervalBetweenPlayoffRoundGames,
                intervalBetweenPlayoffRoundGamesHours = document.intervalBetweenPlayoffRoundGamesHours,
                intervalBetweenRounds = document.intervalBetweenRounds,
                intervalBetweenRoundsHours = document.intervalBetweenRoundsHours,
                playoffContention = document.playoffContention,
                playoffEligibleLimit = document.playoffEligibleLimit,
                PlayoffSizeLimit = document.PlayoffSizeLimit,
                PlayoffSeries = document.PlayoffSeries,
                SeriesLengthMax = document.SeriesLengthMax,
                sameSeriesLength = document.sameSeriesLength,
                BreakTiesViaGame = document.BreakTiesViaGame,
                otherMetrics = document.otherMetrics
            };
        }
        if (db == "leagueSeasonAssignments") {
            doc = new LeaguePlayerSeasonAssignments {
                AssignmentsId = document.AssignmentsId,
                ConfigId = document.ConfigId,
                LeagueId = document.LeagueId,
                PartitionsEnabled = document.PartitionsEnabled,
                ReassignEverySeason = document.ReassignEverySeason,
                AutomaticInduction = document.AutomaticInduction,
                NumberOfPlayersPerPartition = document.NumberOfPlayersPerPartition,
                NumberOfPartitions = document.NumberOfPartitions,
                SamePartitionSize = document.SamePartitionSize,
                AutomaticScheduling = document.AutomaticScheduling,
                ExcludeOutsideGames = document.ExcludeOutsideGames,
                InterDivisionGameLimit = document.InterDivisionGameLimit,
                RepeatMatchups = document.RepeatMatchups,
                MaxRepeatMatchups = document.MaxRepeatMatchups
            };
        }
        if (db == "leaguePlayoffConfig") {
            doc = new LeaguePlayoffs {
                LeagueId = document.LeagueId,
                RandomInitialMode = document.RandomInitialMode,
                RandomRoundMode = document.RandomRoundMode,
                WholeMode = document.WholeMode,
                DefaultMode = document.DefaultMode,
                WholeRoundOrdering = document.WholeRoundOrdering,
                WholePlayoffPairings = document.WholePlayoffPairings,
                CombinedDivisionGroups = document.CombinedDivisionGroups,
                CombinedDivisionPlayoffMatchups = document.CombinedDivisionPlayoffMatchups,
                DivisonBasedPlayoffPairings = document.DivisionBasedPlayoffPairings,
                DivisionBasedPlayoffMatchups = document.DivisionBasedPlayoffMatchups,
                UserDefinedPlayoffMatchups = document.UserDefinedPlayoffMatchups,
                UserDefinedPlayoffFinalGroups = document.UserDefinedPlayoffFinalGroups
            };
        }

        var db_collection = client.GetDatabase("league").GetCollection<BsonDocument>(db);
        db_collection.InsertOne((BsonDocument) doc);
    }

    public void EditData(String db, bool upsert, Dictionary<string, object> newValues) {

    }
    public void DeleteData(String db, Dictionary<String, object> filter) {

    }

}