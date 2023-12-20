namespace CompetitiveGamingApp.Models;

public class SingleGamePaymentTransactions {
    public String? transactionId {get; set;}
    public String? initPlayer {get; set;}
    public String? hostPlayer {get; set;}
    public String? gameId {get; set;}
    public String? playerLost {get; set;}
    public int amountPaid {get; set;}
    public DateTime timePaid {get; set;}
}