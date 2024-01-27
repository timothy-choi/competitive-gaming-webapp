using MongoDB.Driver;
using MongoDB.Bson;


namespace CompetitiveGamingApp.Services;


public class MongoDBService {
    private readonly MongoClient client;
    public MongoDBService(IConfiguration configuration) {
        client = new MongoClient(configuration.GetConnectionString("MongoDB_URI"));
        List<String> dbs = client.ListDatabaseNames().ToList();
        if (!dbs.Contains("league")) {

        }
    }
}