using StackExchange.Redis;
using System;

namespace ApiServices.LeagueAssignmentApi.RedisServer;

public class RedisConnector
{
    private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
    {
        return ConnectionMultiplexer.Connect("localhost:6379");
    });

    public static ConnectionMultiplexer Connection
    {
        get
        {
            return lazyConnection.Value;
        }
    }

    public static IDatabase db {
        get {
            return Connection.GetDatabase();
        }
    }
}