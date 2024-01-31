namespace CompetitiveGamingApp.Controller;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/League")]
public class LeagueController : ControllerBase {
    private readonly MongoDBService _leagueService;
    public LeagueController(MongoDBService leagueService) {
        _leagueService = leagueService;
    }

    [HttpGet]
    public async Task<ActionResult<List<League>>> GetAllLeagues() {
       var allLeagues = await _leagueService.GetAllData("leagueInfo");
       OkObjectResult res = new OkObjectResult(allLeagues);
       return Ok(allLeagues); 
    }

    [HttpGet("{LeagueId}")]
    public async Task<ActionResult<League>> GetLeagueById(string LeagueId) {
        var league = await _leagueService.GetData("leagueInfo", LeagueId);
        OkObjectResult res = new OkObjectResult(league);
        return Ok(res);
    }

    [HttpPost]
    public async Task<ActionResult> CreateLeague(Dictionary<string, object> leagueInput) {
        try {
            League curr = new League {
                LeagueId = Guid.NewGuid().ToString(),
                Name = leagueInput["LeagueName"],
                Owner = leagueInput["LeagueOwner"],
                Description = leagueInput["LeagueDescription"],
                Players = new List<Dictionary<String, Object?>>(),
                tags = new List<string>(),
                LeagueConfig = "",
                SeasonAssignments = "",
                LeagueStandings = new List<Tuple<String, Dictionary<String, Dictionary<String, object>>>>(),
                AchieveLeagueStandings = new List<List<Tuple<String, Dictionary<String, Dictionary<String, object>>>>>(),
                DivisionStandings = new Dictionary<String, List<Tuple<String, Dictionary<String, object>>>>(),
                AchieveDivisionStandings = new List<Dictionary<String, List<Tuple<String, Dictionary<String, object>>>>>(),
                CombinedDivisionStandings = new Dictionary<String, List<Tuple<String, Dictionary<String, object>>>>(),
                AchieveCombinedDivisionStandings = new List<Dictionary<String, List<Tuple<String, Dictionary<String, object>>>>>(),
                Champions = new List<Tuple<String, String>>(),
                PlayoffAssignments = ""
            };

            await _leagueService.PostData("leagueInfo", curr);
            OkObjectResult res = new OkObjectResult(curr.LeagueId);
            return Ok(res);
        }
        catch {
            return BadRequest();
        }
    }


}


