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

    [HttpPost]
    public async Task<ActionResult<String>> CreateLeaguePlayoffs(Dictionary<string, object> reqBody) {
        try {
            LeaguePlayoffs currPlayoffs = new LeaguePlayoffs {
                LeaguePlayoffId = Guid.NewGuid().ToString(),
                LeagueId = reqBody["LeagueId"] as String,
                RandomInitialMode = Convert.ToBoolean(reqBody["RandomInitialMode"]),
                RandomRoundMode = Convert.ToBoolean(reqBody["RandomRoundMode"]),
                WholeMode = Convert.ToBoolean(reqBody["WholeMode"]),
                DefaultMode = Convert.ToBoolean(reqBody["DefaultMode"]),
                CombinedDivisionMode = Convert.ToBoolean(reqBody["CombinedDivisionMode"]),
                WholeRoundOrdering = new List<Tuple<int, Tuple<string, string>>>(),
                CombinedDivisionGroups = new List<Tuple<string, List<Tuple<int, Tuple<string, string>>>>>(),
                DivisionBasedPlayoffPairings = new List<Tuple<string, Tuple<int, Tuple<string, string>>>>(),
                UserDefinedPlayoffMatchups = new List<Tuple<int, Tuple<string, string>>>(),
                FinalPlayoffBracket = new PlayoffBracket(),
                ArchievePlayoffBrackets = new List<Tuple<int, PlayoffBracket?>>()
            };

            await _leagueService.PostData("leaguePlayoffConfig", currPlayoffs);

            OkObjectResult res = new OkObjectResult(currPlayoffs.LeaguePlayoffId);

            return Ok(res);
        } catch {
            return BadRequest();
        }
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

    [HttpPut("{LeaguePlayoffId}/{LeagueId}")]
    public async Task<ActionResult> AddLeagueId(string LeaguePlayoffId, string LeagueId) {
        try {
            var playoffs = (LeaguePlayoffs) await _leagueService.GetData("leaguePlayoffConfig", LeaguePlayoffId);
            if (playoffs == null) {
                return BadRequest();
            }

            Dictionary<string, bool> upsertOpt = new Dictionary<string, bool>();
            upsertOpt["LeagueId"] = true;
            Dictionary<string, object> updatedData = new Dictionary<string, object>();
            updatedData["LeagueId"] = LeagueId;

            await _leagueService.EditData("leaguePlayoffConfig", upsertOpt, updatedData);

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("{LeaguePlayoffId}/Modes")]
    public async Task<ActionResult> EditLeaguePlayoffModes(string LeaguePlayoffId, [FromBody] Dictionary<string, object> reqBody) {
        try {
            var playoffs = (LeaguePlayoffs) await _leagueService.GetData("leaguePlayoffConfig", LeaguePlayoffId);
            if (playoffs == null) {
                return BadRequest();
            }

            Dictionary<string, bool> upsertOpt = new Dictionary<string, bool>();
            Dictionary<string, object> updatedData = new Dictionary<string, object>();
            foreach (var mode in reqBody) {
                upsertOpt["LeagueId"] = true;
                updatedData[mode.Key] = mode.Value;
            }

            await _leagueService.EditData("leaguePlayoffConfig", upsertOpt, updatedData);

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("{LeaguePlayoffId}/WholeMode")]
    public async Task<ActionResult> CreateWholeModePlayoffModeFormat(string LeaguePlayoffId, Dictionary<string, object> reqBody) {
        try {
            var playoffs = (LeaguePlayoffs) await _leagueService.GetData("leaguePlayoffConfig", LeaguePlayoffId);
            if (playoffs == null) {
                return BadRequest();
            }

            List<Tuple<int, Tuple<String, String>>> WholeModePlayoffOrdering = new List<Tuple<int, Tuple<String, String>>>();

            if (playoffs.DefaultMode) {
                int index = 1;
                foreach (var round in reqBody) {
                    foreach (var matchup in (List<Tuple<String, String>>) round.Value) {
                        WholeModePlayoffOrdering.Add(Tuple.Create(index, matchup));
                    }
                    index++;
                }
            }
            else {
                int complex_index = 1;
                foreach (var round in reqBody) {
                    foreach (var matchup in (List<Tuple<String, String>>) round.Value) {
                        string teamA = matchup.Item1;
                        string teamB = matchup.Item2;
                        if (matchup.Item1.StartsWith("BYE")) {
                            teamA = "ROUND:" + (complex_index-1) + "INDEX:" + matchup.Item1.Substring(3);
                        }
                        if (matchup.Item2.StartsWith("BYE")) {
                            teamB = "ROUND:" + (complex_index-1) + "INDEX:" + matchup.Item1.Substring(3);
                        }
                        if (matchup.Item1.EndsWith("BYE")) {
                            teamA = "ROUND:" + (complex_index-1) + "INDEX:" + matchup.Item1.Substring(3);
                        }
                        if (matchup.Item2.EndsWith("BYE")) {
                            teamB = "ROUND:" + (complex_index-1) + "INDEX:" + matchup.Item1.Substring(3);
                        }
                        Tuple<string, string> modifiedMatchup = new Tuple<string, string>(teamA, teamB);
                        WholeModePlayoffOrdering.Add(Tuple.Create(complex_index, modifiedMatchup));
                    }
                    complex_index++;
                }
            }

            Dictionary<string, bool> upsertOpt = new Dictionary<string, bool>();
            Dictionary<string, object> updatedData = new Dictionary<string, object>();

            upsertOpt["WholeRoundOrdering"] = false;

            updatedData["WholeRoundOrdering"] = WholeModePlayoffOrdering;

            await _leagueService.EditData("leaguePlayoffConfig", upsertOpt, updatedData);

            return Ok();
        } catch {
            return BadRequest();
        }
    }
}