namespace CompetitiveGamingApp.Services;

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
        } catch {
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
            string cmd = "INSERT INTO public.singleGames (SingleGameId, hostPlayer, guestPlayer, finalScore, inGameScores, timePlayed, videoObjName, gameEditor, twitchBroadcasterId) VALUES (@SingleGameId, @hostPlayer, @guestPlayer, @finalScore, @inGameScores, @timePlayed, @videoObjName, @gameEditor, @twitchBroadcasterId)";
            await _dbServices.EditData<SingleGame>(cmd, currMatch);
        } catch {
            throw new Exception("Couldn't create game!");
        }
    }
    public async Task UpdateFinalScore(Tuple<int> completedScore, string gameId) {
        try {
            string cmd = "UPDATE public.singleGames SET finalScore = @completedScore WHERE SingleGameId = @gameId";
            await _dbServices.EditData<SingleGame>(cmd, new {completedScore, gameId});
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
    public async Task AddInGameScores(Tuple<String, Tuple<int, int>> score, string gameId) {
        try {
            string cmd = "SELECT * FROM public.singleGames WHERE SingleGameId = @gameId";
            SingleGame? game = await _dbServices.GetAsync<SingleGame>(cmd, new {gameId});
            game?.inGameScores!.Add(score);
            var scoresArr = game?.inGameScores!.ToArray();
            cmd = "UPDATE public.singleGames SET inGameScores = @scoresArr WHERE SingleGameId = @gameId";
            await _dbServices.EditData<SingleGame>(cmd, new {scoresArr, gameId});
        } catch {
            throw new Exception("Couldn't add in-game score");
        }
    }
}