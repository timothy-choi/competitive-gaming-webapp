namespace CompetitiveGamingApp.Controller;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/LeagueConfig")]
public class LeagueSeasonConfigController : ControllerBase {
    private readonly MongoDBService _leagueService;

    public LeagueSeasonConfigController(MongoDBService leagueService) {
        _leagueService = leagueService;
    }

    [HttpGet("{ConfigId}")]
    public async Task<ActionResult<LeagueSeasonConfig>> GetLeagueSeasonConfig(string ConfigId) {
        var config = _leagueService.GetData("leagueConfig", ConfigId);
        if (config.Count == 0) {
            return NotFound();
        }

        OkObjectResult res = new OkObjectResult(config);
        return Ok(res);
    }
}