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
    private readonly IDbConnection _db_singleGame;

    public DBService(IConfiguration configuration) {
        _db_gameHistory = new NpgsqlConnection(configuration.GetConnectionString("PostgresSqlGameHistoryTable"));
        _db_singleGame = new NpgsqlConnection(configuration.GetConnectionString("PostgresSqlSingleGameTable"));
    }

    public async Task<T?> GetAsync<T>(string cmd, object parms) {
        T? result = default;
        if (typeof(T) == typeof(SingleGamePaymentTransactions)) {
            result = (await _db_gameHistory.QueryAsync<T>(cmd, parms)).FirstOrDefault();
            if (result == null) throw new Exception("Data not found");
        }
        if (typeof(T) == typeof(SingleGame)) {
            result = (await _db_singleGame.QueryAsync<T>(cmd, parms)).FirstOrDefault();
            if (result == null) throw new Exception("Data not found");
        }
        return result;
    }

    public async Task<List<T>?> GetAll<T>(string cmd, object parms) {
        List<T>? result = default;
        if (typeof(T) == typeof(SingleGamePaymentTransactions)) {
            result = (await _db_gameHistory.QueryAsync<T>(cmd, parms)).ToList();
            if (result == null) throw new Exception("Data not found");
        }
        if (typeof(T) == typeof(SingleGame)) {
            result = (await _db_singleGame.QueryAsync<T>(cmd, parms)).ToList();
            if (result == null) throw new Exception("Data not found");
        }
        return result;
    }

    public async Task<int> EditData<T>(string cmd, object parms) {
        int result = default;
        if (typeof(T) == typeof(SingleGamePaymentTransactions)) {
            result = await _db_gameHistory.ExecuteAsync(cmd, parms);
        }
        if (typeof(T) == typeof(SingleGame)) {
            result = await _db_singleGame.ExecuteAsync(cmd, parms);
        }
        return result;
    }
}
