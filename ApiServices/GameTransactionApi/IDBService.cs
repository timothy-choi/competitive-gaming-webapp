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
    private readonly IDbConnection _db_gameHistory;

    public DBService(IConfiguration configuration) {
        _db_gameHistory = new NpgsqlConnection(configuration.GetConnectionString("DefaultConnection"));
    }

    public async Task<T?> GetAsync<T>(string cmd, object parms) {
        T? result = default;
        if (typeof(T) == typeof(SingleGamePaymentTransactions)) {
            result = (await _db_gameHistory.QueryAsync<T>(cmd, parms)).FirstOrDefault();
            if (result == null) throw new Exception("Data not found");
        }
        return result;
    }

     public async Task<List<T>?> GetAll<T>(string cmd, object parms)
{
    var result = await _db_gameHistory.QueryAsync<T>(cmd, parms);
    return result?.ToList() ?? new List<T>();
}

    public async Task<int> EditData<T>(string cmd, object parms) {
        int result = default;
        if (typeof(T) == typeof(SingleGamePaymentTransactions)) {
            result = await _db_gameHistory.ExecuteAsync(cmd, parms);
        }
        return result;
    }
}
