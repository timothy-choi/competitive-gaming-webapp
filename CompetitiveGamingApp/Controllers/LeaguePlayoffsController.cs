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
        var playoffs = (LeaguePlayoffs) await _leagueService.GetData("leaguePlayoffConfig", LeaguePlayoffId);
        if (playoffs == null) {
            return BadRequest();
        }

        OkObjectResult res = new OkObjectResult(playoffs);

        return Ok(res);
    }


    [HttpDelete("{LeaguePlayoffId}")]
    public async Task<ActionResult> DeleteLeaguePlayoffs(string LeaguePlayoffId) {
        try {
            var playoffs = (LeaguePlayoffs) await _leagueService.GetData("leaguePlayoffConfig", LeaguePlayoffId);
            if (playoffs == null) {
                return BadRequest();
            }

            await _leagueService.DeleteData("leaguePlayoffConfig", LeaguePlayoffId);

            return Ok();
        } catch {
            return BadRequest();
        }
    }
}