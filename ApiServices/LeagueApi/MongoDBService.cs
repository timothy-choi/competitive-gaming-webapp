using MongoDB.Driver;
using MongoDB.Bson;
using CompetitiveGamingApp.Models;
using ApiServices.LeagueApi;
using ApiServices.LeagueApi.MongoDBSettings;
using MongoDB.Bson.Serialization;



namespace ApiServices.LeagueApi.MongoDBService;


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

    public async Task<List<League>> GetAllData(string db) {
        var db_collection = client.GetDatabase("league").GetCollection<BsonDocument>(db);
        if (db == "leagueInfo" || db == "leagueConfig" || db == "leagueSeasonAssignments" || db == "leaguePlayoffs") {
            var filter = Builders<BsonDocument>.Filter.Empty;
            List<BsonDocument> bsonDocuments = await db_collection.Find(filter).ToListAsync();

           List<League> leagues = bsonDocuments
            .Select(doc => BsonSerializer.Deserialize<League>(doc)) // Use BsonSerializer to deserialize to League
            .ToList();

            return leagues;
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