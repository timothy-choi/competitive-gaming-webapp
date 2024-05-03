namespace CompetitiveGamingApp.Controllers;

using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Services;


[ApiController]
[Route("api/Recommendation")]


public class RecommendationController : ControllerBase {
    private PlayerRecommendationServices _playerServices;

    private LeagueRecommendationServices _leagueServices;
    public RecommendationController(PlayerRecommendationServices playerServices, LeagueRecommendationServices leagueServices) {
        _playerServices = playerServices;
        _leagueServices = leagueServices;
    }

    [HttpPost("{playerUsername}/PlayerRecord")]
    public async Task<Action> AddNewPlayerRecord(string playerUsername) {
        
    }



}


