namespace CompetitiveGamingApp.Services;

using Microsoft.EntityFrameworkCore;
using CompetitiveGamingApp.Models;

public class PlayerAuthServices : DbContext {
    public PlayerAuthServices(DbContextOptions<PlayerAuthServices> options) 
    :base(options) 
    {

    }
    public DbSet<PlayerAuth> playerAuths {get; set;}
}