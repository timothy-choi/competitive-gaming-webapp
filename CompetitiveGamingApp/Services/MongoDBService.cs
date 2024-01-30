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
            var filterById = Builders<LeagueInfo>.Filter.Eq(league => league.LeagueId, entityId);
            var res = db_collection.Find(filterById).FirstOrDefault();
            return res;
        }
        if (db == "leagueConfig") {
            var filterById = Builders<LeagueConfig>.Filter.Eq(league => league.ConfigId, entityId);
            var res = db_collection.Find(filterById).FirstOrDefault();
            return res;
        }
        if (db == "leagueSeasonAssignments") {
            var filterById = Builders<LeagueSeasonConfig>.Filter.Eq(league => league.AssignmentsId, entityId);
            var res = db_collection.Find(filterById).FirstOrDefault();
            return res;
        }
        var filterById = Builders<LeaguePlayoffConfig>.Filter.Eq(league => league.LeaguePlayoffId, entityId);
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
                MaxRepeatMatchups = document.MaxRepeatMatchups,
                DivisionSelective = document.DivisionSelective,
                OutsideDivisionSelections = document.OutsideDivisionSelections,
                AllPartitions = document.AllPartitions,
                PlayerFullSchedule = document.PlayerFullSchedule,
                FinalFullSchedule  = document.FinalFullSchedule
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

    private void updateDocument(string KeyName, object update, Dictionary<string, bool> upsertChangeStatus, Dictionary<string, object> newValues) {
        if (upsertChangeStatus[KeyName]) {
            update = update.Push(KeyName, newValues[KeyName]);
        }
        else {
            update = update.Set(KeyName, newValues[KeyName]);
        }
    }

    public void EditData(String db, Dictionary<string, bool> upsertChangeStatus, Dictionary<string, object> newValues) {
        var db_collection = client.GetDatabase("league").GetCollection<BsonDocument>(db);
        var filter = Builder<BsonDocument>.Filter.Eq(newValues["IdName"], newValues["id"]);
        object update = Builders<BsonDocument>.Update;
        for (int i = 0; i < newValues.Keys.Count; ++i) {
            updateDocument(newValues.Keys[i], update, upsertChangeStatus, newValues);
        }
        db_collection.UpdateOne(filter, update);
    }
    public void DeleteData(String db, string docId) {
        var db_collection = client.GetDatabase("league").GetCollection<BsonDocument>(db);
        string IdName = "";
        if (db == "leagueInfo") {
            IdName = "LeagueId";
        }
        if (db == "leagueConfig") {
            IdName = "ConfigId";
        } 
        if (db == "leagueSeasonAssignments") {
            IdName = "AssignmentsId";
        }
        if (db == "leaguePlayoffConfig") {
            IdName = "LeaguePlayoffId";
        }
        var filter = Builders<BsonDocument>.Filter.Eq(IdName, docId);
        db_collection.DeleteOne(filter);
    }
}