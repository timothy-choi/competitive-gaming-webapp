namespace CompetitiveGamingApp.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
using Dapper;
using Npgsql;
using System.Data;


using CompetitiveGamingApp.Models;

public class SingleGameServices {
    private readonly IDBService _dbServices;

    public SingleGameServices(IDBService dbServices) {
        _dbServices = dbServices;
    }

  public async Task<List<SingleGame>> GetAllGames()
{
    try
    {
        string cmd = "SELECT * FROM public.singleGames";
        List<SingleGame> allGames = await _dbServices.GetAll<SingleGame>(cmd, new {});

        return allGames; // Returns an empty list if no data is found
    }
    catch (Exception e)
    {
        Console.WriteLine($"Error fetching all games: {e.Message}");
        throw new Exception("Couldn't get all games!", e); // Include the original exception as inner exception
    }
}



    public async Task<SingleGame?> GetGame(string gameId) {
        try {
            string cmd = "SELECT * FROM public.singleGames WHERE SingleGameId = @gameId";
            SingleGame? game = await _dbServices.GetAsync<SingleGame>(cmd, new {gameId});
            return game;
        } catch {
            throw new Exception("Couldn't get game!");
        }
    }

public async Task CreateGame(SingleGame currMatch)
{
    try
    {
        // SQL command string
        string cmd = "INSERT INTO public.singleGames " +
                     "(SingleGameId, hostPlayer, guestPlayer, hostScore, guestScore, " +
                     "timePlayed, gameEditor, twitchBroadcasterId, otherGameInfo, predictionId, videoFilePath) " +
                     "VALUES (@SingleGameId, @hostPlayer, @guestPlayer, @hostScore, @guestScore, " +
                     "@timePlayed, @gameEditor, @twitchBroadcasterId, @otherGameInfo, @predictionId, @videoFilePath)";

        // Create Dapper parameters
        var parameters = new DynamicParameters();
        parameters.Add("@SingleGameId", currMatch.SingleGameId);
        parameters.Add("@hostPlayer", currMatch.hostPlayer);
        parameters.Add("@guestPlayer", currMatch.guestPlayer);
        parameters.Add("@hostScore", currMatch.hostScore);
        parameters.Add("@guestScore", currMatch.guestScore);

        // Pass TimeSpan directly, let Dapper/Npgsql handle conversion to INTERVAL
        parameters.Add("@timePlayed", currMatch.timePlayed); // No string conversion needed
        parameters.Add("@gameEditor", currMatch.gameEditor);
        parameters.Add("@twitchBroadcasterId", currMatch.twitchBroadcasterId);
        parameters.Add("@otherGameInfo", currMatch.otherGameInfo);
        parameters.Add("@predictionId", currMatch.predictionId);
        parameters.Add("@videoFilePath", currMatch.videoFilePath);

        // Execute the query with Dapper
        await _dbServices.EditData<SingleGame>(cmd, parameters);
    }
    catch (Exception ex)
    {
        // Log the exception for debugging
        Console.WriteLine($"Error inserting game: {ex.Message}");
        throw new Exception("Couldn't create game!", ex); // Include original exception as inner exception
    }
}



    public async Task DeleteGame(string gameId) {
        try {
            string cmd = "DELETE FROM public.singleGames WHERE SingleGameId = @gameId";
            await _dbServices.EditData<SingleGame>(cmd, new {gameId});
        } catch {
            throw new Exception("Couldn't create game!");
        }
    }
    public async Task UpdateFinalScore(int hostScore, int guestScore, string gameId) {
        try {
            string cmd = "UPDATE public.singleGames SET hostScore = @hostScore WHERE SingleGameId = @gameId";
            await _dbServices.EditData<SingleGame>(cmd, new {hostScore, gameId});

            cmd = "UPDATE public.singleGames SET guestScore = @guestScore WHERE SingleGameId = @gameId";
            await _dbServices.EditData<SingleGame>(cmd, new {guestScore, gameId});
        }
        catch {
            throw new Exception("Couldn't add final score");
        }
    }
    public async Task EditUserGameEditor(string editorUsername, string gameId) {
        try {
            string cmd = "UPDATE public.singleGames SET gameEditor = @editorUsername WHERE SingleGameId = @gameId";
            await _dbServices.EditData<SingleGame>(cmd, new {editorUsername, gameId});
        } catch {
            throw new Exception("Couldn't name game editor");
        }
    }



    public async Task AddOtherGameInfo(Dictionary<String, String> gameInfo, string gameId) {
        try {
            string cmd = "UPDATE public.singleGames SET otherGameInfo = @gameInfo WHERE SingleGameId = @gameId";
            await _dbServices.EditData<SingleGame>(cmd, new {gameInfo, gameId});
        } catch {
            throw new Exception("Couldn't add other game info");
        }
    }

    public async Task AddTwitchBroadcasterId(string twitchId, string gameId) {
        try {
            string cmd = "UPDATE public.singleGames SET twitchBroadcasterId = @twitchId WHERE SingleGameId = @gameId";
            await _dbServices.EditData<SingleGame>(cmd, new {twitchId, gameId});
        } catch {
            throw new Exception("Couldn't add twitch id");
        }
    }

    public async Task AddPredictionId(string  predId, string gameId) {
        try {
            string cmd = "UPDATE public.singleGames SET predId = @predictionId WHERE SingleGameId = @gameId";
            await _dbServices.EditData<SingleGame>(cmd, new {predId, gameId});
        } catch {
            throw new Exception("Couldn't add prediction id");
        }
    }

     public async Task AddVideoFilePath(string videoPath, string gameId) {
        try {
            string cmd = "UPDATE public.singleGames SET videoFilePath = @videoPath WHERE SingleGameId = @gameId";
            await _dbServices.EditData<SingleGame>(cmd, new {videoPath, gameId});
        } catch {
            throw new Exception("Couldn't add prediction id");
        }
    }
}