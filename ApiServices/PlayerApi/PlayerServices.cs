namespace CompetitiveGamingApp.Services;

using Microsoft.EntityFrameworkCore;
using CompetitiveGamingApp.Models;

public class PlayerServices : DbContext {
     public PlayerServices(DbContextOptions<PlayerServices> options)
        : base(options)
    {
    }

    public DbSet<Player> players {get; set;}
}