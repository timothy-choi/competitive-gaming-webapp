using CompetitiveGamingApp.Models;

namespace CompetitiveGamingApp.Services;


public class SingleGamePaymentTransactionsService {
    private readonly IDBService _dbServices;

    public SingleGamePaymentTransactionsService(IDBService dbService) {
        _dbServices = dbService;
    }

    public async Task<List<SingleGamePaymentTransactions>?> getAllGames() {
        try {
            string cmd = "SELECT * FROM single_game_payment_transactions";
            List<SingleGamePaymentTransactions>? allItems = await _dbServices.GetAll<SingleGamePaymentTransactions>(cmd, new {});
            return allItems;
        } catch {
            throw new Exception("Couldn't get all game history");
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