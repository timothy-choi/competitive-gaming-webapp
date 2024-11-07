using CompetitiveGamingApp.Models;

namespace CompetitiveGamingApp.Services;


public class SingleGamePaymentTransactionsService {
    private readonly IDBService _dbServices;

    public SingleGamePaymentTransactionsService(IDBService dbService) {
        _dbServices = dbService;
    }

    public async Task<List<SingleGamePaymentTransactions>?> getAllGames() {
        try
        {
            string cmd = "SELECT * FROM public.GameHistory";
            List<SingleGamePaymentTransactions> allGames = await _dbServices.GetAll<SingleGamePaymentTransactions>(cmd, new {});

            return allGames; // Returns an empty list if no data is found
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error fetching all games: {e.Message}");
            throw new Exception("Couldn't get all games!", e); // Include the original exception as inner exception
        }
    }

    public async Task<SingleGamePaymentTransactions?> getPastGame(string transId) {
        try {
            string cmd = "SELECT * FROM public.GameHistory WHERE transactionId = @transId";
            SingleGamePaymentTransactions? trans = await _dbServices.GetAsync<SingleGamePaymentTransactions>(cmd, new {transId});
            return trans;
        } catch {
            throw new Exception("Couldn't get game history");
        }
    }

    public async Task addNewGameResult(SingleGamePaymentTransactions newTrans) {
        try {
            string cmd = "INSERT INTO public.GameHistory (TransactionId, initPlayer, hostPlayer, gameId, playerLost, amountPaid, timePaid, paymentId) VALUES (@TransactionId, @initPlayer, @hostPlayer, @gameId, @playerLost, @amountPaid, @timePaid, @paymentId)";
            await _dbServices.EditData<SingleGamePaymentTransactions>(cmd, newTrans);
        } catch {
            throw new Exception("Couldn't add new game history");
        }
    }
}