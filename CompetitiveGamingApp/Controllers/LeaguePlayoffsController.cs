namespace CompetitiveGamingApp.Controllers;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;


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

    private  Dictionary<string, List<Tuple<String, String>>> ParsePlayoffFormat(string PlayoffFormat) {
        var playoffs = new Dictionary<string, List<Tuple<String, String>>>();

        using (var streamReader = new StreamReader(PlayoffFormat)) {
            string line;
            string round = "";
            int round_num = 1;
            while ((line = streamReader.ReadLine()) != null) {
               if (line.StartsWith("ROUND")) {
                round = line.Substring(0, 5) + round_num;
                round_num++;
               }
               else {
                if (!line.Contains(" ") && !line.Contains(",")) {
                    throw new Exception("Invalid Playoff Formatting");
                }

                if (line.Contains(",")) {
                    List<string> gameInfo = line.Split(',').ToList();
                    playoffs[round].Add(Tuple.Create(gameInfo[0], gameInfo[1]));
                }
                else {
                    List<string> gameInfo = line.Split(' ').ToList();
                    playoffs[round].Add(Tuple.Create(gameInfo[0], gameInfo[1]));
                }
               }
            }
        }

        return playoffs;
    }

    private bool VerifyPlayoffFormat( Dictionary<string, List<Tuple<String, String>>> playoffs, int player_count, bool defaultMode) {
        int ct = player_count / 2;
        int r = 1;
        if (defaultMode) {
            while (playoffs.ContainsKey("ROUND" + r)) {
                if (playoffs["ROUND" + r].Count == ct) {
                    ct /= 2;
                    var seen = new Dictionary<string, bool>();
                    foreach (var matchup in playoffs["ROUND" + r]) {
                        if (r == 1) {
                            if (matchup.Item1.Contains("/") || matchup.Item2.Contains("/") || !int.TryParse(matchup.Item1, out _) || !int.TryParse(matchup.Item2, out _)) {
                                return false;
                            }
                            if (int.TryParse(matchup.Item1, out int res)) {
                                if (!(res >= 1 && res <= player_count)) {
                                    return false;
                                }
                            }
                            if (int.TryParse(matchup.Item2, out int res2)) {
                                if (!(res2 >= 1 && res2 <= player_count)) {
                                    return false;
                                }
                            }
                            if (seen[matchup.Item1] || seen[matchup.Item2]) {
                                return false;
                            }
                            seen[matchup.Item1] = true;
                            seen[matchup.Item2] = true;
                        }
                        else {
                            if (matchup.Item1.Contains("/")) {
                                var arr = matchup.Item1.Split("/").ToList();
                                if (arr.Count > 2 || arr.Count < 2) {
                                    return false;
                                }
                                foreach (var elt in arr) {
                                    if (!int.TryParse(elt, out _) || seen[elt]) {
                                        return false;
                                    }
                                    if (int.TryParse(elt, out int res)) {
                                        if (!(res >= 1 && res <= player_count)) {
                                            return false;
                                        }
                                    }
                                    seen[elt] = true;
                                }
                            }
                            if (matchup.Item2.Contains("/")) {
                                var arr = matchup.Item2.Split("/").ToList();
                                if (arr.Count > 2 || arr.Count < 2) {
                                    return false;
                                }
                                foreach (var elt in arr) {
                                    if (!int.TryParse(elt, out _) || seen[elt]) {
                                        return false;
                                    }
                                    if (int.TryParse(elt, out int res)) {
                                        if (!(res >= 1 && res <= player_count)) {
                                            return false;
                                        }
                                    }
                                    seen[elt] = true;
                                }
                            }
                            else {
                                if (seen[matchup.Item1] || seen[matchup.Item2] || !int.TryParse(matchup.Item1, out _) || !int.TryParse(matchup.Item2, out _)) {
                                    return false;
                                }
                                if (int.TryParse(matchup.Item1, out int res)) {
                                    if (!(res >= 1 && res <= player_count)) {
                                        return false;
                                    }
                                }
                                if (int.TryParse(matchup.Item2, out int res2)) {
                                    if (!(res2 >= 1 && res2 <= player_count)) {
                                        return false;
                                    }
                                }
                                seen[matchup.Item1] = true;
                                seen[matchup.Item2] = true;
                            }
                        }
                    }
                }
                else {
                    return false;
                }
                r++;
            }
            if (ct > 0) {
                return false;
            }
        }
        else {
            while (playoffs.ContainsKey("ROUND" + r)) {
                if (r == 1) {
                    if (!playoffs.ContainsKey("ROUND2")) {
                        return false;
                    }
                    int bye_count = 0;
                    for (int i = 0; i < playoffs["ROUND2"].Count; ++i) {
                        if (playoffs["ROUND2"][i].Item1.Contains("BYE")) {
                            bye_count++;
                        } 
                        if (playoffs["ROUND2"][i].Item2.Contains("BYE")) {
                            bye_count++;
                        } 
                    }

                    if (bye_count != playoffs["ROUND1"].Count) {
                        return false;
                    }

                    var seen = new Dictionary<string, bool>();

                    foreach (var matchup in playoffs["ROUND1"]) {
                        if (matchup.Item1.Contains("/") || !int.TryParse(matchup.Item1, out _) || matchup.Item2.Contains("/") || !int.TryParse(matchup.Item2, out _)) {
                            return false;
                        }

                        if (int.TryParse(matchup.Item1, out int res)) {
                            if (!(res >= 1 && res <= player_count)) {
                                return false;
                            }
                        }
                        if (int.TryParse(matchup.Item2, out int res2)) {
                            if (!(res2 >= 1 && res2 <= player_count)) {
                                return false;
                            }
                        }

                        if (seen[matchup.Item1] || seen[matchup.Item2]) {
                            return false;
                        }

                        seen[matchup.Item1] = true;
                        seen[matchup.Item2] = true;
                    }

                    ct -= bye_count * 2;
                }
                else if (playoffs["ROUND" + r].Count == ct) {
                    var seen = new Dictionary<string, bool>();
                    foreach (var matchup in playoffs["ROUND" + r]) {
                        if (matchup.Item1.Contains("/")) {
                            var arr = matchup.Item1.Split("/").ToList();
                            if (arr.Count > 2 || arr.Count < 2) {
                                return false;
                            }
                            foreach (var elt in arr) {
                                if (!int.TryParse(elt, out _) || seen[elt]) {
                                    return false;
                                }
                                if (int.TryParse(elt, out int res)) {
                                    if (!(res >= 1 && res <= player_count)) {
                                        return false;
                                    }
                                }
                                seen[elt] = true;
                            }
                        }
                        if (matchup.Item2.Contains("/")) {
                            var arr = matchup.Item2.Split("/").ToList();
                            if (arr.Count > 2 || arr.Count < 2) {
                                return false;
                            }
                            foreach (var elt in arr) {
                                if (!int.TryParse(elt, out _) || seen[elt]) {
                                    return false;
                                }
                                if (int.TryParse(elt, out int res)) {
                                    if (!(res >= 1 && res <= player_count)) {
                                        return false;
                                    }
                                }
                                seen[elt] = true;
                            }
                        }
                        else {
                            if (seen[matchup.Item1] || seen[matchup.Item2] || !int.TryParse(matchup.Item1, out _) || !int.TryParse(matchup.Item1, out _) || !matchup.Item2.StartsWith("BYE") || !matchup.Item2.StartsWith("BYE")) {
                                return false;
                            }

                            if (matchup.Item1.StartsWith("BYE") && matchup.Item1.Substring(3).GetType() == typeof(String)) {
                                if (!int.TryParse(matchup.Item1.Substring(3), out int res3) && !(res3 >= 0 && res3 <= playoffs["ROUND" + (r-1)].Count - 1)) {
                                    return false;
                                }
                            }
                            else {
                                return false;
                            }
                            if (matchup.Item2.StartsWith("BYE") && matchup.Item2.Substring(3).GetType() == typeof(String)) {
                                if (!int.TryParse(matchup.Item2.Substring(3), out int res4) && !(res4 >= 1 && res4 <= playoffs["ROUND" + (r-1)].Count - 1)) {
                                    return false;
                                }
                            }
                            else {
                                return false;
                            }

                            if (matchup.Item1.GetType() == typeof(int) && int.TryParse(matchup.Item1, out int res)) {
                                if (!(res >= 1 && res <= player_count)) {
                                    return false;
                                }
                            }
                            if (matchup.Item2.GetType() == typeof(int) && int.TryParse(matchup.Item2, out int res2)) {
                                if (!(res2 >= 1 && res2 <= player_count)) {
                                    return false;
                                }
                            }
                            seen[matchup.Item1] = true;
                            seen[matchup.Item2] = true;
                        }
                    }
                    if (ct > 0) {
                        return false;
                    }
                }
                else {
                    return false;
                }
                r++;
                ct /= 2;
            }
            if (ct > 0) {
                return false;
            }
        }
        
        return true;
    }

    [HttpPost("{LeaguePlayoffId}/WholeMode/ProcessUserFile")]
    public async Task<ActionResult<Dictionary<string, object>>> ProcessUserSubmittedWholeSchedule(string LeaguePlayoffId, [FromForm] IFormFile PlayoffFormat, [FromBody] Dictionary<string, object> reqBody) {
        try {
            var playoffs = (LeaguePlayoffs) await _leagueService.GetData("leaguePlayoffConfig", LeaguePlayoffId);
            if (playoffs == null) {
                return BadRequest();
            }

            if (PlayoffFormat == null || PlayoffFormat.Length == 0) {
                return BadRequest();
            }

            int player_count = Convert.ToInt32(reqBody["PlayerCount"]);

            bool defaultMode = Convert.ToBoolean(reqBody["DefaultMode"]);

            Dictionary<string, List<Tuple<String, String>>> parsedPlayoffs = new Dictionary<string, List<Tuple<String, String>>>();

            using (var streamReader = new StreamReader(PlayoffFormat.OpenReadStream())) {
                var fileContent = await streamReader.ReadToEndAsync();

                parsedPlayoffs = ParsePlayoffFormat(fileContent);
            }

            if (!VerifyPlayoffFormat(parsedPlayoffs, player_count, defaultMode)) {
                return BadRequest();
            }

            Dictionary<string, object> reqPlayoffs = new Dictionary<string, object>();

            foreach (var round in parsedPlayoffs) {
                reqPlayoffs[round.Key] = round.Value;
            }

            OkObjectResult res = new OkObjectResult(reqPlayoffs);

            return Ok(res);
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