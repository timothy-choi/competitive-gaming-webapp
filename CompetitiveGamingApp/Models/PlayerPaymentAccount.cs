namespace CompetitiveGamingApp.Models;

public class PlayerPaymentAccount {
    public String? playerPaymentAccountId {get; set;}
    public String? playerUsername {get; set;}
    public String? playerPaymentUsername {get; set;}
    public String? playerCashAppId {get; set;}
    public String? MerchantId {get; set;}
    public String? idempotencyKey {get; set;}
};