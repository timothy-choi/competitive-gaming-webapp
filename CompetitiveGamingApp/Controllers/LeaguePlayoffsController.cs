namespace CompetitiveGamingApp.Controllers;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;
using System;


[ApiController]
[Route("api/LeaguePlayoffs")]
public class LeaguePlayoffsController : ControllerBase {
    private readonly MongoDBService _leagueService;

    public LeaguePlayoffsController(MongoDBService leagueService) {
        _leagueService = leagueService;
    }

    [HttpGet("{LeaguePlayoffId}")]
    public async Task<ActionResult<LeaguePlayoffs>> GetLeaguePlayoffs(string LeaguePlayoffId) {
        var playoffs = _leagueService.GetData("leaguePlayoffConfig", LeaguePlayoffId);
        if (playoffs == null) {
            return BadRequest();
        }

        OkObjectResult res = new OkObjectResult(playoffs);

        return Ok(res);
    }
}