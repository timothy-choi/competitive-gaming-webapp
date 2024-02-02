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

    [HttpPost]
    public async Task<ActionResult<string>> CreateLeagueSeasonConfig(Dictionary<string, object> reqBody) {
        try {
            LeagueSeasonConfig createConfig = new LeagueSeasonConfig {
                ConfigId = Guid.NewGuid().ToString(),
                LeagueName = reqBody["LeagueName"],
                commitmentLength = reqBody["commitmentLength"],
                feePrice = reqBody["feePrice"],
                NumberOfPlayersLimit = reqBody["NumberOfPlayersLimit"],
                NumberOfPlayersMin = reqBody["NumberOfPlayersMin"],
                JoinDuringSeason = reqBody["JoinDuringSeason"],
                convertToRegular = reqBody["convertToRegular"],
                seasons = reqBody["seasons"],
                NumberOfGames = reqBody["NumberOfGames"],
                selfScheduleGames = reqBody["selfScheduleGames"],
                intervalBetweenGames = reqBody["intervalBetweenGames"],
                intervalBetweenGamesHours = reqBody["intervalBetweenGames"],
                firstSeasonMatch = reqBody["firstSeasonMatch"],
                tiesAllowed = reqBody["tiesAllowed"],
                playoffStart = reqBody["playoffStart"],
                intervalBetweenPlayoffRoundGames = reqBody["intervalBetweenPlayoffRoundGames"],
                intervalBetweenPlayoffRoundGamesHours = reqBody["intervalBetweenPlayoffRoundGamesHours"],
                intervalBetweenRounds = reqBody["intervalBetweenRounds"], 
                intervalBetweenRoundsHours = reqBody["intervalBetweenRoundsHours"],
                playoffContention = reqBody["playoffContention"],
                playoffEligibleLimit = reqBody["playoffEligibleLimit"],
                PlayoffSizeLimit = reqBody["playoffSizeLimit"],
                PlayoffSeries = reqBody["PlayoffSeries"],
                SeriesLengthMax = reqBody["SeriesLengthMax"],
                sameSeriesLength = reqBody["sameSeriesLength"],
                GamesPerRound = reqBody["GamesPerRound"],
                BreakTiesViaGame = reqBody["BreakTiesViaGame"],
                otherMetrics = reqBody["otherMetrics"]
            };

            await _leagueService.PostData("leagueConfig", createConfig);
            OkObjectResult res = new OkObjectResult(createConfig.ConfigId);

            return Ok(res);
        }
        catch {
            return BadRequest();
        }
    }

    [HttpDelete("{ConfigId}")]
    public async Task<ActionResult> DeleteLeagueSeasonConfig(string ConfigId) {
        try {
            var config = _leagueService.GetData("leagueConfig", ConfigId);
            if (config.Count == 0) {
                return NotFound();
            }

            await _leagueService.DeleteData("leagueConfig", ConfigId);

            return Ok();
        }
        catch {
            return BadRequest();
        }
    }
}