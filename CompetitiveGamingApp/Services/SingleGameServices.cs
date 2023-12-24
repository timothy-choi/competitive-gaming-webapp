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
}