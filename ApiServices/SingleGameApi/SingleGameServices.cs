namespace CompetitiveGamingApp.Services;
using Newtonsoft.Json;

using CompetitiveGamingApp.Models;

public class SingleGameServices {
    private readonly IDBService _dbServices;

    public SingleGameServices(IDBService dbServices) {
        _dbServices = dbServices;
    }
    public async Task<List<SingleGame>?> GetAllGames() {
        try {
            string cmd = "SELECT * FROM public.singleGames";
            List<SingleGame>? allGames = await _dbServices.GetAll<SingleGame>(cmd, new {});
            return allGames;
        } catch (Exception e) {
            Console.WriteLine(e);
            throw new Exception("Couldn't get all games!");
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

    public async Task CreateGame(SingleGame currMatch) {
        try {
            string cmd = "INSERT INTO public.singleGames (SingleGameId, hostPlayer, guestPlayer, finalScore, inGameScores, timePlayed, gameEditor, twitchBroadcasterId, otherGameInfo, predictionId, videoFilePath) VALUES (@SingleGameId, @hostPlayer, @guestPlayer, @finalScore, @inGameScores, @timePlayed, @gameEditor, @twitchBroadcasterId, @otherGameInfo, @predictionId, @videoFilePath)";
            await _dbServices.EditData<SingleGame>(cmd, currMatch);
        } catch {
            throw new Exception("Couldn't create game!");
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
   
   public async Task AddInGameScores(InGameScore score, string gameId)
{
    try
    {
        // Fetch the game by gameId
        string cmd = "SELECT * FROM public.singleGames WHERE SingleGameId = @gameId";
        SingleGame? game = await _dbServices.GetAsync<SingleGame>(cmd, new { gameId });
        
        // Add the new score to the game's InGameScores list
        game?.inGameScores!.Add(score);

        // Update the game with the new scores (Dapper will handle JSON conversion)
        cmd = "UPDATE public.singleGames SET inGameScores = @InGameScores WHERE SingleGameId = @gameId";
        await _dbServices.EditData<SingleGame>(cmd, new { game.inGameScores, gameId });
    }
    catch
    {
        throw new Exception("Couldn't add in-game score");
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