using MongoDB.Driver;
using MongoDB.Bson;
using CompetitiveGamingApp.Models;
using ApiServices.LeagueAssignmentApi;
using ApiServices.LeagueAssignmentApi.MongoDBSettings;
using MongoDB.Bson.Serialization;


namespace ApiServices.LeagueAssignmentApi.MongoDBService;


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

    public async Task<List<LeaguePlayerSeasonAssignments>> GetAllData(string db) {
        var db_collection = client.GetDatabase("league").GetCollection<BsonDocument>(db);
        if (db == "leagueInfo" || db == "leagueConfig" || db == "leagueSeasonAssignments" || db == "leaguePlayoffs") {
           var filter = Builders<BsonDocument>.Filter.Empty;
            List<BsonDocument> bsonDocuments = await db_collection.Find(filter).ToListAsync();

           List<LeaguePlayerSeasonAssignments> leagues = bsonDocuments
            .Select(doc => BsonSerializer.Deserialize<LeaguePlayerSeasonAssignments>(doc)) // Use BsonSerializer to deserialize to League
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
                { "playAllPlayers", assignments.playAllPlayers },
                { "AllPartitions", BsonDocumentWrapper.Create(assignments.AllPartitions ?? new Dictionary<string, List<string>>()) },
                { "AllCombinedDivisions", BsonDocumentWrapper.Create(assignments.AllCombinedDivisions ?? new Dictionary<string, List<string>>()) },
                { "PlayerFullSchedule", BsonDocumentWrapper.Create(assignments.PlayerFullSchedule ?? new List<Tuple<string, List<object>>>()) },
                { "ArchievePlayerFullSchedule", BsonDocumentWrapper.Create(assignments.ArchievePlayerFullSchedule ?? new List<List<Tuple<string, List<object>>>>()) },
                { "FinalFullSchedule", BsonDocumentWrapper.Create(assignments.FinalFullSchedule ?? new List<SingleGame>()) },
                { "ArchieveFinalFullSchedule", BsonDocumentWrapper.Create(assignments.ArchieveFinalFullSchedule ?? new List<List<SingleGame>>()) }
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