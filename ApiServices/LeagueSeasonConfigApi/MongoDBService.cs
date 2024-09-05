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
        if (db == "leagueInfo" || db == "leagueConfig" || db == "leagueSeasonAssignments" || db == "leaguePlayoffs") {
            var filter = Builders<BsonDocument>.Filter.Empty;
            List<BsonDocument> bsonDocuments = await db_collection.Find(filter).ToListAsync();
            return bsonDocuments.Cast<object>().ToList();
        }
        else {
            throw new ArgumentException("Invalid collection name");
        }
    }

    public async Task<object> GetData(String db, String entityId) {
        var db_collection = client.GetDatabase("league").GetCollection<BsonDocument>(db);
    
        if (db == "leagueInfo") {
            var filterById = Builders<BsonDocument>.Filter.Eq("LeagueId", entityId);
            BsonDocument bsonDocument = await db_collection.Find(filterById).FirstOrDefaultAsync();
            return bsonDocument?.AsBsonDocument;
        }
        else if (db == "leagueConfig") {
            var filterById = Builders<BsonDocument>.Filter.Eq("ConfigId", entityId);
            BsonDocument bsonDocument = await db_collection.Find(filterById).FirstOrDefaultAsync();
            return bsonDocument?.AsBsonDocument;
        }
        else if (db == "leagueSeasonAssignments") {
            var filterById = Builders<BsonDocument>.Filter.Eq("AssignmentsId", entityId);
            BsonDocument bsonDocument = await db_collection.Find(filterById).FirstOrDefaultAsync();
            return bsonDocument?.AsBsonDocument;
        }
        else if (db == "leaguePlayoffs") {
            var filterById = Builders<BsonDocument>.Filter.Eq("LeaguePlayoffId", entityId);
            BsonDocument bsonDocument = await db_collection.Find(filterById).FirstOrDefaultAsync();
            return bsonDocument?.AsBsonDocument;
        }
        else {
            throw new ArgumentException("Invalid collection name");
        }
    }

    public async Task PostData(string db, object document) {
    BsonDocument doc;

    switch (db) {
        case "leagueInfo":
            League leagueInfo = (League)document;
            doc = new BsonDocument {
                { "LeagueId", leagueInfo.LeagueId },
                { "Name", leagueInfo.Name },
                { "Owner", leagueInfo.Owner },
                { "Description", leagueInfo.Description },
                { "Players", new BsonArray(leagueInfo.Players ?? new List<Dictionary<string, object?>>()) },
                { "tags", new BsonArray(leagueInfo.tags ?? new List<string?>()) },
                { "LeagueConfig", leagueInfo.LeagueConfig },
                { "SeasonAssignments", leagueInfo.SeasonAssignments },
                { "LeagueStandings", leagueInfo.LeagueStandings != null ? BsonDocument.Parse(leagueInfo.LeagueStandings.ToJson()) : BsonNull.Value },
                { "AchieveLeagueStandings", new BsonArray(leagueInfo.AchieveLeagueStandings ?? new List<LeagueTable>()) },
                { "DivisionStandings", new BsonDocument(leagueInfo.DivisionStandings ?? new Dictionary<string, DivisionTable>()) },
                { "ArchieveDivisionStandings", new BsonArray(leagueInfo.ArchieveDivisionStandings ?? new List<Dictionary<string, DivisionTable>>()) },
                { "CombinedDivisionStandings", new BsonDocument(leagueInfo.CombinedDivisionStandings ?? new Dictionary<string, CombinedDivisionTable>()) },
                { "ArchieveCombinedDivisionStandings", new BsonArray(leagueInfo.ArchieveCombinedDivisionStandings ?? new List<Dictionary<string, CombinedDivisionTable>>()) },
                { "Champions", new BsonArray(leagueInfo.Champions ?? new List<Tuple<string, string>>()) },
                { "PlayoffAssignments", leagueInfo.PlayoffAssignments },
                { "Season", leagueInfo.Season },
                { "StartDate", leagueInfo.StartDate }
            };
            break;

        case "leagueConfig":
            LeagueSeasonConfig leagueConfig = (LeagueSeasonConfig)document;
            doc = new BsonDocument {
                { "ConfigId", leagueConfig.ConfigId },
                { "LeagueName", leagueConfig.LeagueName },
                { "commitmentLength", leagueConfig.commitmentLength },
                { "feePrice", leagueConfig.feePrice },
                { "NumberOfPlayersLimit", leagueConfig.NumberOfPlayersLimit },
                { "OwnerAsPlayer", leagueConfig.OwnerAsPlayer },
                { "NumberOfPlayersMin", leagueConfig.NumberOfPlayersMin },
                { "NumberOfGames", leagueConfig.NumberOfGames },
                { "selfScheduleGames", leagueConfig.selfScheduleGames },
                { "intervalBetweenGames", leagueConfig.intervalBetweenGames },
                { "intervalBetweenGamesHours", leagueConfig.intervalBetweenGamesHours },
                { "firstSeasonMatch", new BsonArray(leagueConfig.firstSeasonMatch ?? new List<Tuple<string, DateTime>>()) },
                { "playoffStartOffset", leagueConfig.playoffStartOffset },
                { "intervalBetweenPlayoffRoundGames", leagueConfig.intervalBetweenPlayoffRoundGames },
                { "intervalBetweenPlayoffRoundGamesHours", leagueConfig.intervalBetweenPlayoffRoundGamesHours },
                { "intervalBetweenRounds", leagueConfig.intervalBetweenRounds },
                { "intervalBetweenRoundsHours", leagueConfig.intervalBetweenRoundsHours },
                { "playoffContention", leagueConfig.playoffContention },
                { "playoffEligibleLimit", leagueConfig.playoffEligibleLimit },
                { "PlayoffSizeLimit", leagueConfig.PlayoffSizeLimit },
                { "PlayoffSeries", leagueConfig.PlayoffSeries },
                { "SeriesLengthMax", leagueConfig.SeriesLengthMax },
                { "sameSeriesLength", leagueConfig.sameSeriesLength },
                { "GamesPerRound", new BsonArray(leagueConfig.GamesPerRound ?? new List<int>()) },
                { "otherMetrics", new BsonArray(leagueConfig.otherMetrics ?? new List<string>()) }
            };
            break;

        case "leagueSeasonAssignments":
            LeaguePlayerSeasonAssignments assignments = (LeaguePlayerSeasonAssignments)document;
            doc = new BsonDocument {
                { "AssignmentsId", assignments.AssignmentsId },
                { "ConfigId", assignments.ConfigId },
                { "LeagueId", assignments.LeagueId },
                { "PartitionsEnabled", assignments.PartitionsEnabled },
                { "ReassignEverySeason", assignments.ReassignEverySeason },
                { "AutomaticInduction", assignments.AutomaticInduction },
                { "NumberOfPlayersPerPartition", assignments.NumberOfPlayersPerPartition },
                { "NumberOfPartitions", assignments.NumberOfPartitions },
                { "AutomaticScheduling", assignments.AutomaticScheduling },
                { "ExcludeOutsideGames", assignments.ExcludeOutsideGames },
                { "InterDvisionGameLimit", assignments.InterDvisionGameLimit },
                { "RepeatMatchups", assignments.RepeatMatchups },
                { "MaxRepeatMatchups", assignments.MaxRepeatMatchups },
                { "DivisionSelective", assignments.DivisionSelective },
                { "OutsideDivisionSelections", BsonDocumentWrapper.Create(assignments.OutsideDivisionSelections ?? new Dictionary<string, List<string>>()) },
                { "RandomizeDivisionSelections", assignments.RandomizeDivisionSelections },
                { "PlayerSelection", assignments.PlayerSelection },
                { "PlayerExemptLists", BsonDocumentWrapper.Create(assignments.PlayerExemptLists ?? new Dictionary<string, List<string>>()) },
                { "repeatAllMatchups", assignments.repeatAllMatchups },
                { "minRepeatMatchups", assignments.minRepeatMatchups },
                { "maxRepeatMatchups", assignments.maxRepeatMatchups },
                { "playAllPlayers", assignments.playAllPlayers },
                { "AllPartitions", BsonDocumentWrapper.Create(assignments.AllPartitions ?? new Dictionary<string, List<string>>()) },
                { "AllCombinedDivisions", BsonDocumentWrapper.Create(assignments.AllCombinedDivisions ?? new Dictionary<string, List<string>>()) },
                { "PlayerFullSchedule", BsonDocumentWrapper.Create(assignments.PlayerFullSchedule ?? new List<Tuple<string, List<object>>>()) },
                { "ArchievePlayerFullSchedule", BsonDocumentWrapper.Create(assignments.ArchievePlayerFullSchedule ?? new List<List<Tuple<string, List<object>>>>()) },
                { "FinalFullSchedule", BsonDocumentWrapper.Create(assignments.FinalFullSchedule ?? new List<SingleGame>()) },
                { "ArchieveFinalFullSchedule", BsonDocumentWrapper.Create(assignments.ArchieveFinalFullSchedule ?? new List<List<SingleGame>>()) }
            };
            break;

        case "leaguePlayoffConfig":
            LeaguePlayoffs playoffs = (LeaguePlayoffs)document;
            doc = new BsonDocument {
                { "LeaguePlayoffId", playoffs.LeaguePlayoffId },
                { "LeagueId", playoffs.LeagueId },
                { "RandomInitialMode", playoffs.RandomInitialMode },
                { "RandomRoundMode", playoffs.RandomRoundMode },
                { "WholeMode", playoffs.WholeMode },
                { "DefaultMode", playoffs.DefaultMode },
                { "CombinedDivisionMode", playoffs.CombinedDivisionMode },
                { "DivisonMode", playoffs.DivisionMode},
                { "WholeRoundOrdering", BsonDocumentWrapper.Create(playoffs.WholeRoundOrdering ?? new List<Tuple<int, Tuple<string, string>>>()) },
                { "CombinedDivisionGroups", BsonDocumentWrapper.Create(playoffs.CombinedDivisionGroups ?? new List<Tuple<string, List<Tuple<int, Tuple<string, string>>>>>()) },
                { "DivisionBasedPlayoffPairings", BsonDocumentWrapper.Create(playoffs.DivisionBasedPlayoffPairings != null ? new List<Tuple<string, Tuple<int, Tuple<string, string>>>>() : null) },
                { "UserDefinedPlayoffMatchups", BsonDocumentWrapper.Create(playoffs.UserDefinedPlayoffMatchups != null ? new List<Tuple<int, Tuple<string, string>>>() : null) },
                { "PlayoffNames", new BsonArray(playoffs.PlayoffNames ?? new List<string>())}
            };
            break;

        default:
            throw new ArgumentException("Invalid database name");
    }

    var db_collection = client.GetDatabase("league").GetCollection<BsonDocument>(db);

    try {
        await db_collection.InsertOneAsync(doc);
    } catch {
        throw new Exception("Create League Failed!");
    }
}

    public async Task EditData(string db, Dictionary<string, bool> upsertChangeStatus, Dictionary<string, object> newValues) {
        var db_collection = client.GetDatabase("league").GetCollection<BsonDocument>(db);
        var filter = Builders<BsonDocument>.Filter.Eq(newValues["IdName"].ToString(), newValues["id"]);

        var updateBuilder = Builders<BsonDocument>.Update;
        var updateDefinition = updateBuilder.Combine();

        foreach (var key in newValues.Keys) {
            if (key != "IdName" && key != "id") {
                if (upsertChangeStatus.TryGetValue(key, out bool shouldUpsert)) {
                    if (shouldUpsert) {
                        updateDefinition = updateDefinition.Push(key, newValues[key]);
                    }
                    else {
                        updateDefinition = updateDefinition.Set(key, newValues[key]);
                    }
                }
                else {
                    updateDefinition = updateDefinition.Set(key, newValues[key]);
                }
            }
        }

        try {
            await db_collection.UpdateOneAsync(filter, updateDefinition);
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