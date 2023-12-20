namespace CompetitiveGamingApp.Models;

public class PlayerPaymentAccount {
    public String? playerPaymentAccountId {get; set;}
    public String? playerUsername {get; set;}
    public String? playerCashAppId {get; set;}
    public Guid idempotencyKey {get; set;}
};