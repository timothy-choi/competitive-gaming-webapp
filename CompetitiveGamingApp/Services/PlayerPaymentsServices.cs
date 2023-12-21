namespace CompetitiveGamingApp.Services;

using Microsoft.EntityFrameworkCore;
using CompetitiveGamingApp.Models;
public class PlayerPaymentServices : DbContext {
    public PlayerPaymentServices(DbContextOptions<PlayerPaymentServices> options)
    : base(options) 
    {
    }
    public DbSet<PlayerPaymentAccount> PlayerPaymentAccounts {get; set;}
}