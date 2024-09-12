namespace CompetitiveGamingApp.Services;

using Dapper;
using Npgsql;
using System.Data;
using CompetitiveGamingApp.Models;


public interface IDBService {
    Task<T?> GetAsync<T>(string cmd, object parms);
    Task<List<T>?> GetAll<T>(string cmd, object parms);

    Task<int> EditData<T>(string cmd, object parms);
}

public class DBService : IDBService {
    private readonly IDbConnection _db_singleGame;

    public DBService(IConfiguration configuration) {
        _db_singleGame = new NpgsqlConnection(configuration.GetConnectionString("DefaultConnection"));
    }

    public async Task<T?> GetAsync<T>(string cmd, object parms) {
        T? result = default;
        if (typeof(T) == typeof(SingleGame)) {
            result = (await _db_singleGame.QueryAsync<T>(cmd, parms)).FirstOrDefault();
            if (result == null) throw new Exception("Data not found");
        }
        return result;
    }

    public async Task<List<T>?> GetAll<T>(string cmd, object parms)
{
    if (typeof(T) == typeof(SingleGame))
    {
        var result = (await _db_singleGame.QueryAsync<T>(cmd, parms)).ToList();

        if (!result.Any()) // Check if the result list is empty
        {
            throw new Exception("No data found");
        }
        
        return result;
    }

    throw new InvalidOperationException("Type T is not supported");
}

    public async Task<int> EditData<T>(string cmd, object parms) {
        int result = default;
        if (typeof(T) == typeof(SingleGame)) {
            result = await _db_singleGame.ExecuteAsync(cmd, parms);
        }
        return result;
    }
}