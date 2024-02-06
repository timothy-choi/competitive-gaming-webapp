using MongoDB.Driver;
using MongoDB.Bson;
using CompetitiveGamingApp.Models;


namespace CompetitiveGamingApp.Services;


public class MongoDBService {
    private readonly MongoClient client;
    public MongoDBService(IConfiguration configuration) {
        client = new MongoClient(configuration.GetConnectionString("MongoDB_URI"));
    }

    public async Task<List<object>> GetAllData(string db) {
        var db_collection = client.GetDatabase("league").GetCollection<BsonDocument>(db);
        if (db == "leagueInfo") {
            var filter = Builders<League>.Filter.Empty;
            return await db_collection.Find(filter).ToListAsync();
        }
        else if (db == "leagueConfig") {
            var filter = Builders<LeagueSeasonConfig>.Filter.Empty;
            return await db_collection.Find(filter).ToListAsync();
        }
        else if (db == "leagueSeasonAssignments") {
            var filter = Builders<LeaguePlayerSeasonAssignments>.Filter.Empty;
            return await db_collection.Find(filter).ToListAsync();
        }
        var filter = Builders<LeaguePlayoffs>.Filter.Empty;
        return await db_collection.Find(filter).ToListAsync();
    }
    public async Task<object> GetData(String db, String entityId) {
        var db_collection = client.GetDatabase("league").GetCollection<BsonDocument>(db);
        if (db == "leagueInfo") {
            var filterById = Builders<League>.Filter.Eq(league => league.LeagueId, entityId);
            return await db_collection.Find(filterById).FirstOrDefault();
        }
        if (db == "leagueConfig") {
            var filterById = Builders<LeagueSeasonConfig>.Filter.Eq(league => league.ConfigId, entityId);
            return await db_collection.Find(filterById).FirstOrDefault();
        }
        if (db == "leagueSeasonAssignments") {
            var filterById = Builders<LeaguePlayerSeasonAssignments>.Filter.Eq(league => league.AssignmentsId, entityId);
            return await db_collection.Find(filterById).FirstOrDefault();
        }
        var filterById = Builders<LeaguePlayoffs>.Filter.Eq(league => league.LeaguePlayoffId, entityId);
        return await db_collection.Find(filterById).FirstOrDefault();
    }

    public async Task PostData(String db, object document) {
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
                AchieveLeagueStandings = document.ArchieveLeagueStandings,
                ArchieveDivisionStandings = document.ArchieveDivisionStandings,
                ArchieveCombinedDivisionStandings = document.ArchieveCombinedDivisionStandings
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
                OwnerAsPlayer = document.OwnerAsPlayer,
                NumberOfPlayersMin = document.NumberOfPlayersMin,
                JoinDuringSeason = document.JoinDuringSeason,
                convertToRegular = document.convertToRegular,
                seasons = document.seasons,
                NumberOfGames = document.NumberOfGames,
                selfScheduleGames = document.selfScheduleGames,
                intervalBetweenGames = document.intervalBetweenGames,
                intervalBetweenGamesHours = document.intervalBetweenGameHours,
                firstSeasonMatch = document.firstSeasonMatch,
                tiesAllowed = document.tiesAllowed,
                playoffStartOffset = document.playoffStartOffset,
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
                GamesPerRound = document.GamesPerRound,
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
                InterDvisionGameLimit = document.InterDivisionGameLimit,
                RepeatMatchups = document.RepeatMatchups,
                MaxRepeatMatchups = document.MaxRepeatMatchups,
                DivisionSelective = document.DivisionSelective,
                OutsideDivisionSelections = document.OutsideDivisionSelections,
                RandomizeDivisionSelections = document.RandomizeDivisionSelections,
                PlayerSelection = document.PlayerSelection,
                PlayerExemptLists = document.PlayerExemptLists,
                repeatAllMatchups = document.repeatAllMatchups,
                minRepeatMatchups = document.minRepeatMatchups,
                maxRepeatMatchups = document.maxRepeatMatchups,
                playAllPlayers = document.playAllPlayers,
                AllPartitions = document.AllPartitions,
                AllCombinedDivisions = document.AllCombinedDivisions,
                PlayerFullSchedule = document.PlayerFullSchedule,
                ArchievePlayerFullSchedule = document.ArchievePlayerFullSchedule,
                ArchieveFinalFullSchedule = document.ArchieveFinalFullSchedule,
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
                CombinedDivisionMode = document.CombinedDivisionMode,
                WholeRoundOrdering = document.WholeRoundOrdering,
                WholePlayoffPairings = document.WholePlayoffPairings,
                ArchieveWholePlayoffPairings = document.ArchieveWholePlayoffPairings,
                CombinedDivisionGroups = document.CombinedDivisionGroups,
                CombinedDivisionPlayoffMatchups = document.CombinedDivisionPlayoffMatchups,
                ArchieveCombinedDivisionPlayoffMatchups = document.ArchieveCombinedDivisionPlayoffMatchups,
                DivisionBasedPlayoffPairings = document.DivisionBasedPlayoffPairings,
                DivisionBasedPlayoffMatchups = document.DivisionBasedPlayoffMatchups,
                ArchieveDivisionBasedPlayoffMatchups = document.ArchieveDivisionBasedPlayoffMatchups,
                UserDefinedPlayoffMatchups = document.UserDefinedPlayoffMatchups,
                UserDefinedPlayoffFinalGroups = document.UserDefinedPlayoffFinalGroups,
                ArchieveUserDefinedPlayoffFinalGroups = document.ArchieveUserDefinedPlayoffFinalGroups
            };
        }

        var db_collection = client.GetDatabase("league").GetCollection<BsonDocument>(db);
        try {
            await db_collection.InsertOneAsync((BsonDocument) doc);
        } catch {
            throw new Exception("Create League Failed!");
        }
    }

    private void updateDocument(string KeyName, object update, Dictionary<string, bool> upsertChangeStatus, Dictionary<string, object> newValues) {
        if (upsertChangeStatus[KeyName]) {
            update = update.Push(KeyName, newValues[KeyName]);
        }
        else {
            update = update.Set(KeyName, newValues[KeyName]);
        }
    }

    public async Task EditData(String db, Dictionary<string, bool> upsertChangeStatus, Dictionary<string, object> newValues) {
        var db_collection = client.GetDatabase("league").GetCollection<BsonDocument>(db);
        var filter = Builder<BsonDocument>.Filter.Eq(newValues["IdName"], newValues["id"]);
        object update = Builders<BsonDocument>.Update;
        for (int i = 0; i < newValues.Keys.Count; ++i) {
            updateDocument(newValues.Keys[i], update, upsertChangeStatus, newValues);
        }
        try {
            await db_collection.UpdateOneAsync(filter, update);
        }
        catch {
            throw new Exception("Update League Failed!");
        }
    }
    public async Task DeleteData(String db, string docId) {
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
        try {
            await db_collection.DeleteOneAsync(filter);
        } catch {
            throw new Exception("Delete League Failed!");
        }
    }
}