namespace ApiServices.LeaguePlayoffsApi.MongoDBService;


using MongoDB.Driver;
using MongoDB.Bson;
using CompetitiveGamingApp.Models;
using ApiServices.LeaguePlayoffsApi;
using ApiServices.LeaguePlayoffsApi.MongoDBSettings;



public class MongoDBService {
    private readonly MongoClient client;
    public MongoDBService(MongoClient mongoClient, IConfiguration configuration) {
        var mongoDbSettings = configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
        Console.WriteLine($"MongoDB Connection String: {mongoDbSettings.ConnectionString}"); // For debugging

        if (string.IsNullOrEmpty(mongoDbSettings.ConnectionString))
        {
            throw new ArgumentNullException("MongoDB connection string is missing.");
        }

        client = mongoClient;
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
    
        if (db == "leaguePlayoffs") {
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