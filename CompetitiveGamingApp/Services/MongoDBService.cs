using MongoDB.Driver;
using MongoDB.Bson;


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
            doc = new {
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
            doc = new {

            };
        }
        if (db == "leagueSeasonAssignments") {
            doc = new {

            };
        }
        if (db == "leaguePlayoffConfig") {
            doc = new {

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