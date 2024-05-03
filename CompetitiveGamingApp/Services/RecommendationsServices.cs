namespace CompetitiveGamingApp.Services;

using Microsoft.EntityFrameworkCore;
using CompetitiveGamingApp.Models;
public class PlayerRecommendationServices : DbContext {
    public PlayerRecommendationServices(DbContextOptions<PlayerRecommendationServices> options)
    : base(options) 
    {
    }
    public DbSet<PlayerRecommendationServices> RecommendationAccounts {get; set;}
}

public class LeagueRecommendationServices : DbContext {
    public LeagueRecommendationServices(DbContextOptions<LeagueRecommendationServices> options)
    : base(options) 
    {
    }
    public DbSet<LeagueRecommendationServices> RecommendationAccounts {get; set;}
}