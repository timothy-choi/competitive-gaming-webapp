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