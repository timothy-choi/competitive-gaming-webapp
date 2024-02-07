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
        var config = (LeagueSeasonConfig) await _leagueService.GetData("leagueConfig", ConfigId);
        if (config == null) {
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
                LeagueName = reqBody["LeagueName"] as String,
                commitmentLength = Convert.ToInt32(reqBody["commitmentLength"]),
                feePrice = Convert.ToInt32(reqBody["feePrice"]),
                NumberOfPlayersLimit = Convert.ToInt32(reqBody["NumberOfPlayersLimit"]),
                OwnerAsPlayer = Convert.ToBoolean(reqBody["OwnerAsPlayer"]),
                NumberOfPlayersMin = Convert.ToInt32(reqBody["NumberOfPlayersMin"]),
                JoinDuringSeason = Convert.ToBoolean(reqBody["JoinDuringSeason"]),
                convertToRegular = Convert.ToBoolean(reqBody["convertToRegular"]),
                seasons = Convert.ToBoolean(reqBody["seasons"]),
                NumberOfGames = Convert.ToInt32(reqBody["NumberOfGames"]),
                selfScheduleGames = Convert.ToBoolean(reqBody["selfScheduleGames"]),
                intervalBetweenGames = Convert.ToInt32(reqBody["intervalBetweenGames"]),
                intervalBetweenGamesHours = Convert.ToInt32(reqBody["intervalBetweenGames"]),
                firstSeasonMatch = reqBody["firstSeasonMatch"] as List<Tuple<string, DateTime>>,
                tiesAllowed = Convert.ToBoolean(reqBody["tiesAllowed"]),
                playoffStartOffset = Convert.ToInt32(reqBody["playoffStartOffset"]),
                intervalBetweenPlayoffRoundGames = Convert.ToInt32(reqBody["intervalBetweenPlayoffRoundGames"]),
                intervalBetweenPlayoffRoundGamesHours = Convert.ToInt32(reqBody["intervalBetweenPlayoffRoundGamesHours"]),
                intervalBetweenRounds = Convert.ToInt32(reqBody["intervalBetweenRounds"]), 
                intervalBetweenRoundsHours = Convert.ToInt32(reqBody["intervalBetweenRoundsHours"]),
                playoffContention = Convert.ToBoolean(reqBody["playoffContention"]),
                playoffEligibleLimit = Convert.ToBoolean(reqBody["playoffEligibleLimit"]),
                PlayoffSizeLimit = Convert.ToInt32(reqBody["playoffSizeLimit"]),
                PlayoffSeries = Convert.ToBoolean(reqBody["PlayoffSeries"]),
                SeriesLengthMax = Convert.ToInt32(reqBody["SeriesLengthMax"]),
                sameSeriesLength = Convert.ToBoolean(reqBody["sameSeriesLength"]),
                GamesPerRound = reqBody["GamesPerRound"] as List<int>, 
                BreakTiesViaGame = Convert.ToBoolean(reqBody["BreakTiesViaGame"]),
                otherMetrics = reqBody["otherMetrics"] as List<string>
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
            if (config == null) {
                return NotFound();
            }

            await _leagueService.DeleteData("leagueConfig", ConfigId);

            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    [HttpPut("{ConfigId}")]
    public async Task<ActionResult> EditConfigData(string ConfigId, Dictionary<string, object> reqBody) {
        try {
            var config = (LeagueSeasonConfig) await _leagueService.GetData("leagueConfig", ConfigId);
            if (config == null) {
                return NotFound();
            }

            Dictionary<string, bool> upsertOpt = new Dictionary<string, bool>();
            Dictionary<string, object> updatedValues = new Dictionary<string, object>();

            foreach (var setting in reqBody) {
                if (setting.Value is Tuple<bool, bool, int, object> tupleValue) {
                    upsertOpt[setting.Key] = tupleValue.Item1;
                    if (!tupleValue.Item1) {
                        if (tupleValue.Item2) {
                            int pos = tupleValue.Item3;
                            if (setting.Key == "firstSeasonMatch") {
                                var matches = config.firstSeasonMatch;
                                matches.RemoveAt(pos);
                                updatedValues[setting.Key] = matches;
                            }
                            if (setting.Key == "GamesPerRound") {
                                var games = config.GamesPerRound;
                                games.RemoveAt(pos);
                                updatedValues[setting.Key] = games;
                            }
                            if (setting.Key == "otherMetrics") {
                                var metrics = config.otherMetrics;
                                metrics.RemoveAt(pos);
                                updatedValues[setting.Key] = metrics;
                            }
                        }
                        else {
                            updatedValues[setting.Key] = tupleValue.Item4;
                        }
                    }
                    else {
                        updatedValues[setting.Key] = tupleValue.Item4;
                    }
                }
            }


            await _leagueService.EditData("leagueConfig", upsertOpt, updatedValues);

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpGet("{ConfigId}/Metrics")]
    public async Task<ActionResult<List<string>>> GetConfigMetrics(string ConfigId) {
        var config = (LeagueSeasonConfig) await _leagueService.GetData("leagueConfig", ConfigId);
        if (config == null) {
            return NotFound();
        }

        OkObjectResult res = new OkObjectResult(config.otherMetrics);
        return Ok(res);
    }
}