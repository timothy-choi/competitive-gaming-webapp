namespace CompetitiveGamingApp.Controllers;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;



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
                DivisionBasedPlayoffPairings = new List<Tuple<string, List<Tuple<int, Tuple<string, string>>>>>(),
                UserDefinedPlayoffMatchups = new List<Tuple<string, List<Tuple<int, Tuple<string, string>>>>>(),
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
                                if (arr.Count > r || arr.Count < r) {
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
                                if (arr.Count > r || arr.Count < r) {
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
            var usedIndex = new Dictionary<int, bool>();
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
                            if (arr.Count > (r-1) || arr.Count < (r-1)) {
                                return false;
                            }
                            foreach (var elt in arr) {
                                if (!int.TryParse(elt, out _) || seen[elt]) {
                                    return false;
                                }
                                if (elt.Contains("BYE") && (!int.TryParse(elt.Substring(elt.IndexOf("BYE") + 3, elt.LastIndexOf("/", elt.IndexOf("BYE"))), out int result) || !(result > 0 && result <= playoffs["ROUND" + r].Count-1) || usedIndex[result])) {
                                    return false;
                                }
                                if (int.TryParse(elt, out int res)) {
                                    if (!(res >= 1 && res <= player_count)) {
                                        return false;
                                    }
                                }
                                seen[elt] = true;
                                bool parseInt = int.TryParse(elt.Substring(elt.IndexOf("BYE") + 3, elt.LastIndexOf("/", elt.IndexOf("BYE")) - elt.IndexOf("BYE") + 3), out int num);
                                usedIndex[num] = true;
                            }
                        }
                        if (matchup.Item2.Contains("/")) {
                            var arr = matchup.Item2.Split("/").ToList();
                            if (arr.Count > (r-1) || arr.Count < (r-1)) {
                                return false;
                            }
                            foreach (var elt in arr) {
                                if (!int.TryParse(elt, out _) || seen[elt]) {
                                    return false;
                                }
                                if (elt.Contains("BYE") && (!int.TryParse(elt.Substring(elt.IndexOf("BYE") + 3, elt.LastIndexOf("/", elt.IndexOf("BYE")) - elt.IndexOf("BYE") + 3), out int result2) || !(result2 > 0 && result2 <= playoffs["ROUND" + r].Count-1) || usedIndex[result2]))
                                {
                                    return false;
                                }
                                if (int.TryParse(elt, out int res)) {
                                    if (!(res >= 1 && res <= player_count)) {
                                        return false;
                                    }
                                }
                                seen[elt] = true;
                                bool parseInt = int.TryParse(elt.Substring(elt.IndexOf("BYE") + 3, elt.LastIndexOf("/", elt.IndexOf("BYE"))), out int num);
                                usedIndex[num] = true;
                            }
                        }
                        else {
                            if (seen[matchup.Item1] || seen[matchup.Item2] || !int.TryParse(matchup.Item1, out _) || !int.TryParse(matchup.Item1, out _) || !matchup.Item2.StartsWith("BYE") || !matchup.Item2.StartsWith("BYE")) {
                                return false;
                            }

                            if (matchup.Item1.StartsWith("BYE") && matchup.Item1.Substring(3).GetType() == typeof(String)) {
                                if ((!int.TryParse(matchup.Item1.Substring(3), out int res3) && !(res3 >= 0 && res3 <= playoffs["ROUND" + (r-1)].Count - 1)) || usedIndex[res3]) {
                                    return false;
                                }
                                bool parsedInt = int.TryParse(matchup.Item1.Substring(3), out int res5);
                                usedIndex[res5] = true;
                            }
                            else {
                                return false;
                            }
                            if (matchup.Item2.StartsWith("BYE") && matchup.Item2.Substring(3).GetType() == typeof(String)) {
                                if ((!int.TryParse(matchup.Item2.Substring(3), out int res4) && !(res4 >= 1 && res4 <= playoffs["ROUND" + (r-1)].Count - 1)) || usedIndex[res4]) {
                                    return false;
                                }
                                bool parsedInt = int.TryParse(matchup.Item1.Substring(3), out int res6);
                                usedIndex[res6] = true;
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
                            teamB = "ROUND:" + (complex_index-1) + "INDEX:" + matchup.Item2.Substring(3);
                        }
                        if (matchup.Item1.Contains("BYE")) {
                            teamA = teamA.Substring(0, teamA.IndexOf("BYE")) + "ROUND:" + (complex_index-1) + "INDEX:" + matchup.Item1.Substring(teamA.IndexOf("BYE") + 3, matchup.Item1.LastIndexOf("/", matchup.Item1.IndexOf("BYE")) - matchup.Item1.IndexOf("BYE") + 3);
                        }
                        if (matchup.Item2.Contains("BYE")) {
                            teamB = teamB.Substring(0, teamB.IndexOf("BYE")) + "ROUND:" + (complex_index-1) + "INDEX:" + matchup.Item2.Substring(teamB.IndexOf("BYE") + 3, matchup.Item2.LastIndexOf("/", matchup.Item2.IndexOf("BYE")) - matchup.Item2.IndexOf("BYE") + 3);
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

    [HttpPost("{LeaguePlayoffId}/VerifyRandomSubmittedHeadMatchups")]
    public async Task<ActionResult<bool>> VerifyUserSubmittedHeadMatchups(string LeaguePlayoffId, Dictionary<string, object> reqBody) {
        try {
            var playoffs = (LeaguePlayoffs) await _leagueService.GetData("leaguePlayoffConfig", LeaguePlayoffId);
            if (playoffs == null) {
                return BadRequest();
            }

            bool res = true;

            if (playoffs.DefaultMode) {
                double sqrt = Math.Sqrt(Convert.ToInt32(reqBody["num_of_players"]));
                if (!(((List<List<string>>) reqBody["Matchups"]).Count == Convert.ToInt32(reqBody["num_of_players"]) / 2 && (int) sqrt == Math.Sqrt(Convert.ToInt32(reqBody["num_of_players"])))) {
                    res = true;
                }

                var seen = new Dictionary<string, bool>();
                for (int i = 0; i < ((List<List<string>>) reqBody["Matchups"]).Count; ++i) {
                    if (seen[((List<List<string>>) reqBody["Matchups"])[i][0]] || seen[((List<List<string>>) reqBody["Matchups"])[i][1]]) {
                        res = false;
                    }

                    if (Convert.ToInt32(((List<List<string>>) reqBody["Matchups"])[i][0]) < 1 || Convert.ToInt32(((List<List<string>>) reqBody["Matchups"])[i][0]) > Convert.ToInt32(reqBody["num_of_players"])) {
                        res = false;
                    }

                    if (Convert.ToInt32(((List<List<string>>) reqBody["Matchups"])[i][1]) < 1 || Convert.ToInt32(((List<List<string>>) reqBody["Matchups"])[i][1]) > Convert.ToInt32(reqBody["num_of_players"])) {
                        res = false;
                    }

                    seen[((List<List<string>>) reqBody["Matchups"])[i][0]] = true;
                    seen[((List<List<string>>) reqBody["Matchups"])[i][1]] = true;
                }

            }
            else {
                var matchups_roundOne = (List<List<string>>) reqBody["matchups_roundOne"];
                
                var matchups_roundTwo = (List<List<string>>) reqBody["matchups_roundTwo"];

                int bye_count = 0;

                var repeat = new Dictionary<string, bool>();
                var repeatIndex = new Dictionary<int, bool>();

                for (int i = 0; i < matchups_roundOne.Count; ++i) {
                    if (repeat[matchups_roundOne[i][0]] || repeat[matchups_roundOne[i][1]]) {
                        res = false;
                    }

                    if (Convert.ToInt32(matchups_roundOne[i][0]) < 1 || Convert.ToInt32(matchups_roundOne[i][0]) > Convert.ToInt32(reqBody["num_of_players"])) {
                        res = false;
                    }

                    if (Convert.ToInt32(matchups_roundOne[i][1]) < 1 || Convert.ToInt32(matchups_roundOne[i][1]) > Convert.ToInt32(reqBody["num_of_players"])) {
                        res = false;
                    }

                    repeat[((List<List<string>>) reqBody["Matchups"])[i][0]] = true;
                    repeat[((List<List<string>>) reqBody["Matchups"])[i][1]] = true;
                }

                for (int i = 0; i < matchups_roundTwo.Count; ++i) {
                    if (matchups_roundTwo[i][0].Contains("BYE")) {
                        bye_count++;
                        if (!int.TryParse(matchups_roundTwo[i][0].Substring(matchups_roundTwo[i][0].IndexOf("BYE") + 3), out int parsed) || !(parsed >= 1 && parsed < matchups_roundOne.Count) || repeatIndex[parsed]) {
                            res = false;
                        }
                        if (int.TryParse(matchups_roundTwo[i][0].Substring(matchups_roundTwo[i][0].IndexOf("BYE") + 3), out int num)) {
                            repeatIndex[num] = true;
                        }
                    }
                    if (matchups_roundTwo[i][1].Contains("BYE")) {
                        bye_count++;
                        if (!int.TryParse(matchups_roundTwo[i][1].Substring(matchups_roundTwo[i][1].IndexOf("BYE") + 3), out int parsed2) || !(parsed2 >= 1 && parsed2 < matchups_roundOne.Count || repeatIndex[parsed2])) {
                            res = false;
                        }
                        if (int.TryParse(matchups_roundTwo[i][1].Substring(matchups_roundTwo[i][1].IndexOf("BYE") + 3), out int num2)) {
                            repeatIndex[num2] = true;
                        }
                    }
                    if (repeat[matchups_roundTwo[i][0]] || repeat[matchups_roundTwo[i][1]]) {
                        res = false;
                    }

                    if (Convert.ToInt32(matchups_roundTwo[i][0]) < 1 || Convert.ToInt32(matchups_roundTwo[i][0]) > Convert.ToInt32(reqBody["num_of_players"])) {
                        res = false;
                    }

                    if (Convert.ToInt32(matchups_roundTwo[i][1]) < 1 || Convert.ToInt32(matchups_roundTwo[i][1]) > Convert.ToInt32(reqBody["num_of_players"])) {
                        res = false;
                    }

                    repeat[matchups_roundTwo[i][0]] = true;
                    repeat[matchups_roundTwo[i][1]] = true;
                }

                if (bye_count != matchups_roundOne.Count) {
                    res = false;
                }
            }

            OkObjectResult verified = new OkObjectResult(res);

            return Ok(verified);
        } catch {
            return BadRequest();
        }
    }

    private List<List<int>> RandomlyGroup(List<int> objects)
    {
        Random rng = new Random();
        List<int> shuffledObjects = objects.OrderBy(a => rng.Next()).ToList(); // Shuffle the objects randomly
        List<List<int>> groups = new List<List<int>>();
        int groupSize = objects.Count / 2;

        for (int i = 0; i < shuffledObjects.Count; i += groupSize)
        {
            groups.Add(shuffledObjects.GetRange(i, Math.Min(groupSize, shuffledObjects.Count - i))); // Partition into groups
        }

        return groups;
    }

    private List<List<string>> RandomlyGroupTuples(List<string> tuples)
    {
        Random rng = new Random();
        List<string> shuffledTuples = tuples.OrderBy(a => rng.Next()).ToList(); // Shuffle the tuples randomly
        List<List<string>> groups = new List<List<string>>();

        int groupSize = tuples.Count / 2;
        int startIndex = 0;

        for (int i = 0; i < tuples.Count; i += groupSize)
        {
            int remainingCount = tuples.Count - startIndex;
            int currentGroupSize = Math.Min(groupSize, remainingCount);

            List<string> currentGroup = shuffledTuples.GetRange(startIndex, currentGroupSize);
            groups.Add(currentGroup);

            startIndex += currentGroupSize;
        }

        return groups;
    }

    [HttpPost("{LeaguePlayoffId}/WholeMode/Random")] 
    public async Task<ActionResult> RandomSelectionForWholePlayoffs(string LeaguePlayoffId, Dictionary<string, object> reqBody) {
        try {
            var playoffs = (LeaguePlayoffs) await _leagueService.GetData("leaguePlayoffConfig", LeaguePlayoffId);
            if (playoffs == null) {
                return BadRequest();
            }

            List<Tuple<int, Tuple<String, String>>> WholeModePlayoffOrdering = new List<Tuple<int, Tuple<String, String>>>();

            List<List<int>>? groups = new List<List<int>>();

            if (playoffs.DefaultMode) {
                if (playoffs.RandomInitialMode) {
                    int group_num = Convert.ToInt32(reqBody["num_of_players"]);
                    List<int> objects = Enumerable.Range(1, group_num).ToList();

                    groups = RandomlyGroup(objects);
                }
                else {
                    groups = reqBody["groups"] as List<List<int>>;
                }

                foreach (var group in groups) {
                    WholeModePlayoffOrdering.Add(Tuple.Create(1, Tuple.Create(group[0].ToString(), group[1].ToString())));
                }

                int round = 2;

                if (playoffs.RandomRoundMode) {
                    int group_num_size = WholeModePlayoffOrdering.Count;
                    int pos = 0;
                    while (group_num_size >= 2) {
                        List<string> tupleGroups = new List<string>();
                        for (int i = pos; i < WholeModePlayoffOrdering.Count; ++i) {
                            string matchupPlayers = WholeModePlayoffOrdering[i].Item2.Item1 + "/" + WholeModePlayoffOrdering[i].Item2.Item2;
                            tupleGroups.Add(matchupPlayers);
                        }
                        pos = WholeModePlayoffOrdering.Count;
                        List<List<string>> pickedGroups = RandomlyGroupTuples(tupleGroups);
                        foreach (var group in pickedGroups) {
                            WholeModePlayoffOrdering.Add(Tuple.Create(round, Tuple.Create(group[0], group[1])));
                        }
                        group_num_size /= 2;
                    }
                }
                else {
                    int size = WholeModePlayoffOrdering.Count;
                    int curr = 0;
                    while (size >= 2) {
                        for (int group = curr; group < WholeModePlayoffOrdering.Count; group += 2) {
                            string group1 = WholeModePlayoffOrdering[group].Item2.Item1 + "/" + WholeModePlayoffOrdering[group].Item2.Item2;
                            string group2 = WholeModePlayoffOrdering[group+1].Item2.Item1 + "/" + WholeModePlayoffOrdering[group+1].Item2.Item2;
                            WholeModePlayoffOrdering.Add(Tuple.Create(round, Tuple.Create(group1, group2)));
                        }
                        curr = WholeModePlayoffOrdering.Count;
                        size /= 2;
                    }
                }
            }
            else {
                if (playoffs.RandomInitialMode) {
                    int second_round_bye = Convert.ToInt32(reqBody["second_round_bye"]);

                    int first_round_players = Convert.ToInt32(reqBody["first_round_players"]);

                    List<int> nonByeTeams = Enumerable.Range(second_round_bye+1, first_round_players * 2).ToList();

                    List<List<int>> nonByeGroups = RandomlyGroup(nonByeTeams);

                    int round_num = 1;

                    for (int i = 0; i < nonByeGroups.Count; ++i) {
                        WholeModePlayoffOrdering.Add(Tuple.Create(round_num, Tuple.Create(nonByeGroups[i][0].ToString(), nonByeGroups[i][0].ToString())));
                    }

                    var taken = new Dictionary<int, string>();
                    for (int i = 0; i < second_round_bye + first_round_players; ++i) {
                        taken[i+1] = "";
                    }

                    var random = new Random();
                    int ct = 0;
                    List<int> total_spots = Enumerable.Range(1, second_round_bye + first_round_players).ToList();
                    while (ct < nonByeGroups.Count) {
                        var random_spot = total_spots[random.Next(0, total_spots.Count-1)];
                        if (taken.ContainsKey(random_spot)) {
                            continue;
                        }
                        taken[random_spot] = "BYE";
                        ct++;
                    }

                    List<int> allEmptySpots = new List<int>();

                    foreach (var spot in taken) {
                        if (spot.Value == "") {
                            allEmptySpots.Add(spot.Key);
                        }
                    }

                    random = new Random();
                    for (int i = 1; i <= first_round_players; ++i) {
                        var selection = allEmptySpots[random.Next(0, allEmptySpots.Count-1)];
                        allEmptySpots.Remove(selection);
                        taken[selection] = selection.ToString();
                    }

                    random = new Random();

                    for (int i = 0; i < taken.Count; i += 2) {
                        var currPair = taken.ElementAt(i);
                        var first = "";
                        var second = "";
                        if (currPair.Value == "BYE") {
                            var matchupSelection = nonByeGroups[random.Next(0, nonByeGroups.Count-1)];
                            nonByeGroups.Remove(matchupSelection);
                            int found = 0;
                            while (matchupSelection[0] != nonByeGroups[found][0] && matchupSelection[1] != nonByeGroups[found][1]) {
                                found++;
                            }
                            first = "ROUND1INDEX" + found;
                        }
                        else {
                            first = currPair.Value.ToString();
                        }
                        var currPair2 = taken.ElementAt(i+1);
                        if (currPair2.Value == "BYE") {
                            var matchupSelection2 = nonByeGroups[random.Next(0, nonByeGroups.Count-1)];
                            nonByeGroups.Remove(matchupSelection2);
                            int found2 = 0;
                            while (matchupSelection2[0] != nonByeGroups[found2][0] && matchupSelection2[1] != nonByeGroups[found2][1]) {
                                found2++;
                            }
                            second = "ROUND1INDEX" + found2;
                        }
                        else {
                            second = currPair2.Value.ToString();
                        }

                        var newPair = Tuple.Create(first, second);

                        WholeModePlayoffOrdering.Add(Tuple.Create(2, newPair));
                    }
                }
                else {
                    foreach (var matchup in (List<Tuple<string, string>>) reqBody["Round1"]) {
                        WholeModePlayoffOrdering.Add(Tuple.Create(1, Tuple.Create(matchup.Item1, matchup.Item2)));
                    }

                    foreach (var matchup in (List<Tuple<string, string>>) reqBody["Round2"]) {
                        var player1 = matchup.Item1;
                        var player2 = matchup.Item2;
                        if (matchup.Item1.Contains("BYE")) {
                            player1 = "ROUND1INDEX" + matchup.Item1.Substring(3); 
                        }
                        if (matchup.Item2.Contains("BYE")) {
                            player2 = "ROUND1INDEX" + matchup.Item2.Substring(3); 
                        }

                        WholeModePlayoffOrdering.Add(Tuple.Create(2, Tuple.Create(player1, player2)));
                    }
                }

                var round_number = 2;

                int group_num_size = 0;
                for (int i = 0; i < WholeModePlayoffOrdering.Count; ++i) {
                    if (WholeModePlayoffOrdering[i].Item1 == 2) {
                        group_num_size++;
                    }
                }

                if (playoffs.RandomRoundMode) {
                    int pos = 0;
                    while (group_num_size >= 2) {
                        List<string> tupleGroups = new List<string>();
                        for (int i = pos; i < WholeModePlayoffOrdering.Count; ++i) {
                            string matchupPlayers = WholeModePlayoffOrdering[i].Item2.Item1 + "/" + WholeModePlayoffOrdering[i].Item2.Item2;
                            tupleGroups.Add(matchupPlayers);
                        }
                        pos = WholeModePlayoffOrdering.Count;
                        List<List<string>> pickedGroups = RandomlyGroupTuples(tupleGroups);
                        foreach (var group in pickedGroups) {
                            WholeModePlayoffOrdering.Add(Tuple.Create(round_number, Tuple.Create(group[0], group[1])));
                        }
                        group_num_size /= 2;
                    }
                }
                else {
                    int size = group_num_size;
                    int curr = 0;
                    while (size >= 2) {
                        for (int group = curr; group < WholeModePlayoffOrdering.Count; group += 2) {
                            string group1 = WholeModePlayoffOrdering[group].Item2.Item1 + "/" + WholeModePlayoffOrdering[group].Item2.Item2;
                            string group2 = WholeModePlayoffOrdering[group+1].Item2.Item1 + "/" + WholeModePlayoffOrdering[group+1].Item2.Item2;
                            WholeModePlayoffOrdering.Add(Tuple.Create(round_number, Tuple.Create(group1, group2)));
                        }
                        curr = WholeModePlayoffOrdering.Count;
                        size /= 2;
                    }
                }
            }

            Dictionary<string, bool> upsertOpt = new Dictionary<string, bool>();
            upsertOpt["WholeRoundOrdering"] = false;
            Dictionary<string, object> updatedData = new Dictionary<string, object>();
            updatedData["WholeRoundOrdering"] = WholeModePlayoffOrdering;

            await _leagueService.EditData("leaguePlayoffConfig", upsertOpt, updatedData);
            
            return Ok();
        } catch {
            return BadRequest();
        }
    }
    private void ConstructBracket(bool defaultMode, PlayoffBracket leagueBracket, int bracket, List<Tuple<int, Tuple<string, string>>> Ordering) {
        List<Tuple<PlayoffGraphNode, PlayoffGraphNode>> ConnectingRounds = new List<Tuple<PlayoffGraphNode, PlayoffGraphNode>>();
        int node_count = leagueBracket.SubPlayoffBrackets[bracket].PlayoffHeadMatchups.Count;
        if (defaultMode) {
            var nextLevel = new List<PlayoffGraphNode>();
            while (node_count >= 2) {
                if (node_count == leagueBracket.SubPlayoffBrackets[bracket].PlayoffHeadMatchups.Count) {
                    for (int j = 0; j < node_count; j++) {
                        nextLevel.Add(leagueBracket.SubPlayoffBrackets[bracket].PlayoffHeadMatchups[j]);
                    }
                }
                for (int i = 0; i < node_count; i += 2) {
                    PlayoffMatchup playoffMatchup = new PlayoffMatchup();
                    PlayoffGraphNode node = new PlayoffGraphNode(playoffMatchup);
                    ConnectingRounds.Add(Tuple.Create(nextLevel[i], node));
                    ConnectingRounds.Add(Tuple.Create(nextLevel[i+1]!, node));
                }
                nextLevel.RemoveRange(0, node_count);
                var nextNodes = leagueBracket.SubPlayoffBrackets[bracket].ConnectRounds(ConnectingRounds);
                ConnectingRounds = new List<Tuple<PlayoffGraphNode, PlayoffGraphNode>>();
                node_count /= 2;
                for (int i = 0; i < nextNodes.Count; i+=2) {
                    nextLevel.Add(nextNodes[i].Item2);
                }
            }
        }
        else {
            var second_round = Ordering.GetRange(Ordering.IndexOf(Ordering.FirstOrDefault(t => t.Item1 == 2)!), Ordering.Count(tuple => tuple.Item1 == 2));
            var first_round = leagueBracket.SubPlayoffBrackets[bracket].PlayoffHeadMatchups;
            int first_round_index = 0;
            var nextNodes = new List<PlayoffGraphNode>();
            for (int i = 0; i < second_round.Count; ++i) {
                PlayoffMatchup playoffMatchup = new PlayoffMatchup();
                PlayoffGraphNode node = new PlayoffGraphNode(playoffMatchup);
                if (second_round[i].Item2.Item1.Contains("BYE")) {
                    ConnectingRounds.Add(Tuple.Create(first_round[first_round_index], node));
                    var next = leagueBracket.SubPlayoffBrackets[bracket].ConnectRounds(ConnectingRounds);
                    nextNodes.Add(next[0].Item2);
                    first_round_index++;
                }
                else if (second_round[i].Item2.Item2.Contains("BYE")) {
                    ConnectingRounds.Add(Tuple.Create(first_round[first_round_index], node));
                    var next2 = leagueBracket.SubPlayoffBrackets[bracket].ConnectRounds(ConnectingRounds); 
                    nextNodes.Add(next2[0].Item2);
                    first_round_index++;
                }
                if (!second_round[i].Item2.Item1.Contains("BYE") && !second_round[i].Item2.Item2.Contains("BYE")) {
                    nextNodes.Add(node);
                }
            }

            int num_ct = nextNodes.Count;

            int rd = 2;

            while (num_ct >= 2) {
                for (int i = 0; i < num_ct; i += 2) {
                    PlayoffMatchup playoffMatchup = new PlayoffMatchup();
                    PlayoffGraphNode node = new PlayoffGraphNode(playoffMatchup);
                    ConnectingRounds.Add(Tuple.Create(nextNodes[i], node));
                    ConnectingRounds.Add(Tuple.Create(nextNodes[i+1]!, node));
                    leagueBracket.SubPlayoffBrackets[bracket].AllOtherMatchups.Add(Tuple.Create(rd, nextNodes[i]));
                    leagueBracket.SubPlayoffBrackets[bracket].AllOtherMatchups.Add(Tuple.Create(rd, nextNodes[i+1]));
                }
                nextNodes.RemoveRange(0, num_ct);
                var nextRound = leagueBracket.SubPlayoffBrackets[bracket].ConnectRounds(ConnectingRounds);
                ConnectingRounds = new List<Tuple<PlayoffGraphNode, PlayoffGraphNode>>();
                num_ct /= 2;
                for (int i = 0; i < nextRound.Count; i+=2) {
                    nextNodes.Add(nextRound[i].Item2);
                }
                rd++;
            }
        }
    }

    private void SetUpBracket(int wholeOrderingSize, PlayoffBracket leagueBracket, bool defaultMode, List<Tuple<int, Tuple<string, string>>> WholePlayoffFormat, int bracket) {
        List<PlayoffGraphNode> initialHeadMatchups = new List<PlayoffGraphNode>();

        for (int i = 0; i < wholeOrderingSize; i++) {
                if (WholePlayoffFormat[i].Item1 > 1) {
                    break;
                }
                PlayoffMatchup curr = new PlayoffMatchup();
                PlayoffGraphNode HeadNode = new PlayoffGraphNode(curr);
                initialHeadMatchups.Add(HeadNode);
            }

            leagueBracket.SubPlayoffBrackets[bracket].PlayoffHeadMatchups = initialHeadMatchups;

        ConstructBracket(defaultMode, leagueBracket, 0, WholePlayoffFormat);
    }

    private void SetHeadMatchups(PlayoffBracket leagueBracket, bool defaultMode, List<Tuple<int, Tuple<string, string>>> WholePlayoffFormat, int bracket, List<Tuple<int, Dictionary<string, object>>> players) {
        var first_round = WholePlayoffFormat.GetRange(0, WholePlayoffFormat.Count(t => t.Item1 == 1));

        int index = 0;

        for (int i = 0; i < leagueBracket.SubPlayoffBrackets[bracket].PlayoffHeadMatchups.Count; ++i) {
            int rank1 = Convert.ToInt32(first_round[index].Item2.Item1);
            int rank2 = Convert.ToInt32(first_round[index].Item2.Item2);
            leagueBracket.SubPlayoffBrackets[bracket].PlayoffHeadMatchups[i].currentPlayoffMatchup.PlayoffMatchupId = Guid.NewGuid().ToString();
            leagueBracket.SubPlayoffBrackets[bracket].PlayoffHeadMatchups[i].currentPlayoffMatchup.player1 = players[rank1 - 1].Item2["playerName"].ToString();
            leagueBracket.SubPlayoffBrackets[bracket].PlayoffHeadMatchups[i].currentPlayoffMatchup.player1_rank = rank1;
            leagueBracket.SubPlayoffBrackets[bracket].PlayoffHeadMatchups[i].currentPlayoffMatchup.player2 = players[rank2 - 1].Item2["playerName"].ToString();
            leagueBracket.SubPlayoffBrackets[bracket].PlayoffHeadMatchups[i].currentPlayoffMatchup.player2_rank = rank2;
            leagueBracket.SubPlayoffBrackets[bracket].PlayoffHeadMatchups[i].currentPlayoffMatchup.round = 1;
            leagueBracket.SubPlayoffBrackets[bracket].PlayoffHeadMatchups[i].currentPlayoffMatchup.GameId = players[rank1 - 1].Item2["GameId"].ToString();
            leagueBracket.SubPlayoffBrackets[bracket].PlayoffHeadMatchups[i].currentPlayoffMatchup.series_player1_wins = 0;
            leagueBracket.SubPlayoffBrackets[bracket].PlayoffHeadMatchups[i].currentPlayoffMatchup.series_player2_wins = 0;
            leagueBracket.SubPlayoffBrackets[bracket].PlayoffHeadMatchups[i].currentPlayoffMatchup.winner = "";
        }

        if (!defaultMode) {
            var round_two_nodes = leagueBracket.SubPlayoffBrackets[bracket].AllOtherMatchups.GetRange(0, leagueBracket.SubPlayoffBrackets[bracket].AllOtherMatchups.Count(t => t.Item1 == 2));
            var round_two_ordering = WholePlayoffFormat.GetRange(WholePlayoffFormat.IndexOf(WholePlayoffFormat.FirstOrDefault(t => t.Item1 == 2)!), WholePlayoffFormat.Count(tuple => tuple.Item1 == 2));
            for (int i = 0; i < round_two_nodes.Count; ++i) {
                leagueBracket.SubPlayoffBrackets[bracket].AllOtherMatchups[i].Item2.currentPlayoffMatchup.PlayoffMatchupId = Guid.NewGuid().ToString();
                leagueBracket.SubPlayoffBrackets[bracket].AllOtherMatchups[i].Item2.currentPlayoffMatchup.round = 2;
                leagueBracket.SubPlayoffBrackets[bracket].AllOtherMatchups[i].Item2.currentPlayoffMatchup.series_player1_wins = 0;
                leagueBracket.SubPlayoffBrackets[bracket].AllOtherMatchups[i].Item2.currentPlayoffMatchup.series_player2_wins = 0;
                leagueBracket.SubPlayoffBrackets[bracket].AllOtherMatchups[i].Item2.currentPlayoffMatchup.winner = "";
                if (!round_two_ordering[i].Item2.Item1.Contains("BYE")) {
                    int parsedRank1 = Convert.ToInt32(round_two_ordering[i].Item2.Item1);
                    leagueBracket.SubPlayoffBrackets[bracket].AllOtherMatchups[i].Item2.currentPlayoffMatchup.player1 = players[parsedRank1 - 1].Item2["playerName"].ToString();
                    leagueBracket.SubPlayoffBrackets[bracket].AllOtherMatchups[i].Item2.currentPlayoffMatchup.player1_rank = parsedRank1;
                }
                if (!round_two_ordering[i].Item2.Item2.Contains("BYE")) {
                    int parsedRank2 = Convert.ToInt32(round_two_ordering[i].Item2.Item2);
                    leagueBracket.SubPlayoffBrackets[bracket].AllOtherMatchups[i].Item2.currentPlayoffMatchup.player2 = players[parsedRank2 - 1].Item2["playerName"].ToString();
                    leagueBracket.SubPlayoffBrackets[bracket].AllOtherMatchups[i].Item2.currentPlayoffMatchup.player2_rank = parsedRank2;
                }
            }
        } 
    }

    [HttpPost("{LeaguePlayoffId}/WholePlayoffFormatBracket")]
    public async Task<ActionResult> CreateWholePlayoffFormatBracket(string LeaguePlayoffId, Dictionary<string, object> reqBody) {
        try {
            var playoffs = (LeaguePlayoffs) await _leagueService.GetData("leaguePlayoffConfig", LeaguePlayoffId);
            if (playoffs == null) {
                return BadRequest();
            }

            PlayoffBracket leagueBracket = new PlayoffBracket();

            leagueBracket!.AddSubPlayoffBracket(reqBody["PlayoffName"].ToString()!);

            SetUpBracket(playoffs.WholeRoundOrdering!.Count, leagueBracket, playoffs.DefaultMode, playoffs.WholeRoundOrdering, 0);

            List<Tuple<int, Dictionary<string, object>>> allPlayers = new List<Tuple<int, Dictionary<string, object>>>();

            int rank = 1;

            foreach (var pos in (List<Dictionary<string, object>>) reqBody["players"]) {
                allPlayers.Add(Tuple.Create(rank, pos));
            }

            SetHeadMatchups(leagueBracket, playoffs.DefaultMode, playoffs.WholeRoundOrdering, 0, allPlayers);

            Dictionary<string, bool> upsertOpt = new Dictionary<string, bool>();
            upsertOpt["WholeRoundOrdering"] = false;
            Dictionary<string, object> updatedData = new Dictionary<string, object>();
            updatedData["WholeRoundOrdering"] = leagueBracket;

            await _leagueService.EditData("leaguePlayoffConfig", upsertOpt, updatedData);

            return Ok();
        } catch {
            return BadRequest();
        }
    }


    [HttpPost("{LeaguePlayoffId}/ProcessDivisionTypeSubmittedSchedule")]
    public async Task<ActionResult<Dictionary<string, object>>> ProcessUserSubmittedDivisionTypeSchedule(string LeaguePlayoffId, [FromForm] List<IFormFile> allBrackets, [FromBody] Dictionary<string, object> reqBody) {
        try {
            var playoffs = (LeaguePlayoffs) await _leagueService.GetData("leaguePlayoffConfig", LeaguePlayoffId);
            if (playoffs == null) {
                return BadRequest();
            }

            if (allBrackets == null || allBrackets.Count == 0) {
                return BadRequest();
            }

            for (int i = 0; i < allBrackets.Count; ++i) {
                if (allBrackets[i].Length == 0) {
                    return BadRequest();
                }
            }

            int player_count = Convert.ToInt32(reqBody["PlayerCount"]);

            bool defaultMode = Convert.ToBoolean(reqBody["DefaultMode"]);

            Dictionary<string, List<Tuple<String, String>>> parsedPlayoffs = new Dictionary<string, List<Tuple<String, String>>>();

            Dictionary<string, object> reqPlayoffs = new Dictionary<string, object>();

            for (int i = 0; i < allBrackets.Count; ++i) {
                Dictionary<string, object> divisions = new Dictionary<string, object>();
                using (var streamReader = new StreamReader(allBrackets[i].OpenReadStream())) {
                    var fileContent = await streamReader.ReadToEndAsync();

                    parsedPlayoffs = ParsePlayoffFormat(fileContent);
                }

                if (!VerifyPlayoffFormat(parsedPlayoffs, player_count / allBrackets.Count, defaultMode)) {
                    return BadRequest();
                }

                foreach (var round in parsedPlayoffs) {
                    divisions[round.Key] = round.Value;
                }
                reqPlayoffs["bracket" + ((List<String>) reqBody["bracketNames"])[0].ToString()] = divisions;
            }

            OkObjectResult res = new OkObjectResult(reqPlayoffs);

            return Ok(res);
        } catch {
            return BadRequest();
        }
    }

    private List<Tuple<int, Tuple<String, String>>> BuildBracketFromUser(bool defaultMode, Dictionary<string, object> allDivisionBasedBrackets) {
        List<Tuple<int, Tuple<String, String>>> WholeModePlayoffOrdering = new List<Tuple<int, Tuple<String, String>>>();

        if (defaultMode) {
            int index = 1;
            foreach (var round in allDivisionBasedBrackets) {
                foreach (var matchup in (List<Tuple<String, String>>) round.Value) {
                    WholeModePlayoffOrdering.Add(Tuple.Create(index, matchup));
                }
                index++;
            }
        }
        else {
            int complex_index = 1;
            foreach (var round in allDivisionBasedBrackets) {
                foreach (var matchup in (List<Tuple<String, String>>) round.Value) {
                    string teamA = matchup.Item1;
                    string teamB = matchup.Item2;
                    if (matchup.Item1.StartsWith("BYE")) {
                        teamA = "ROUND:" + (complex_index-1) + "INDEX:" + matchup.Item1.Substring(3);
                    }
                    if (matchup.Item2.StartsWith("BYE")) {
                        teamB = "ROUND:" + (complex_index-1) + "INDEX:" + matchup.Item2.Substring(3);
                    }
                    if (matchup.Item1.Contains("BYE")) {
                        teamA = teamA.Substring(0, teamA.IndexOf("BYE")) + "ROUND:" + (complex_index-1) + "INDEX:" + matchup.Item1.Substring(teamA.IndexOf("BYE") + 3, matchup.Item1.LastIndexOf("/", matchup.Item1.IndexOf("BYE")) - matchup.Item1.IndexOf("BYE") + 3);
                    }
                    if (matchup.Item2.Contains("BYE")) {
                        teamB = teamB.Substring(0, teamB.IndexOf("BYE")) + "ROUND:" + (complex_index-1) + "INDEX:" + matchup.Item2.Substring(teamB.IndexOf("BYE") + 3, matchup.Item2.LastIndexOf("/", matchup.Item2.IndexOf("BYE")) - matchup.Item2.IndexOf("BYE") + 3);
                    }
                    Tuple<string, string> modifiedMatchup = new Tuple<string, string>(teamA, teamB);
                    WholeModePlayoffOrdering.Add(Tuple.Create(complex_index, modifiedMatchup));
                }
                complex_index++;
            }
        }

        return WholeModePlayoffOrdering;
    }

    [HttpPost("{LeaguePlayoffsId}/DivisionBasedSchedule")]
    public async Task<ActionResult> CreateDivisionBasedPlayoffModeFormat(string LeaguePlayoffId, Dictionary<string, object> reqBody) {
        try {
            var playoffs = (LeaguePlayoffs) await _leagueService.GetData("leaguePlayoffConfig", LeaguePlayoffId);
            if (playoffs == null) {
                return BadRequest();
            }

            string divisionMode = reqBody["divisonMode"].ToString() ?? String.Empty;

            reqBody.Remove("divisionMode");

            List<Tuple<String, List<Tuple<int, Tuple<String, String>>>>> DivisionBasedBracket = new List<Tuple<String, List<Tuple<int, Tuple<String, String>>>>>();

            foreach (var division in reqBody) {
                List<Tuple<int, Tuple<String, String>>> curr = BuildBracketFromUser(playoffs.DefaultMode, (Dictionary<string, object>) division.Value);
                DivisionBasedBracket.Add(Tuple.Create(division.Key, curr));
            }

            Dictionary<string, bool> upsertOpt = new Dictionary<string, bool>();
            upsertOpt[divisionMode] = false;
            Dictionary<string, object> updatedData = new Dictionary<string, object>();
            updatedData[divisionMode] = DivisionBasedBracket;

            await _leagueService.EditData("leaguePlayoffConfig", upsertOpt, updatedData);

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    private List<Tuple<int, Tuple<String, String>>> RandomGenerateDivsionBasedBracket(bool defaultMode, bool randomInitialMode, bool randomRoundMode, int group_num, Dictionary<string, object> division) {
        List<Tuple<int, Tuple<String, String>>> WholeModePlayoffOrdering = new List<Tuple<int, Tuple<String, String>>>();

        List<List<int>>? groups = new List<List<int>>();

        if (defaultMode) {
            if (randomInitialMode) {
                List<int> objects = Enumerable.Range(1, group_num).ToList();

                groups = RandomlyGroup(objects);
            }
            else {
                groups = division["groups"] as List<List<int>>;
            }

            foreach (var group in groups) {
                WholeModePlayoffOrdering.Add(Tuple.Create(1, Tuple.Create(group[0].ToString(), group[1].ToString())));
            }

            int round = 2;

            if (randomRoundMode) {
                int group_num_size = WholeModePlayoffOrdering.Count;
                int pos = 0;
                while (group_num_size >= 2) {
                    List<string> tupleGroups = new List<string>();
                    for (int i = pos; i < WholeModePlayoffOrdering.Count; ++i) {
                        string matchupPlayers = WholeModePlayoffOrdering[i].Item2.Item1 + "/" + WholeModePlayoffOrdering[i].Item2.Item2;
                        tupleGroups.Add(matchupPlayers);
                    }
                    pos = WholeModePlayoffOrdering.Count;
                    List<List<string>> pickedGroups = RandomlyGroupTuples(tupleGroups);
                    foreach (var group in pickedGroups) {
                        WholeModePlayoffOrdering.Add(Tuple.Create(round, Tuple.Create(group[0], group[1])));
                    }
                    group_num_size /= 2;
                }
            }
            else {
                int size = WholeModePlayoffOrdering.Count;
                int curr = 0;
                while (size >= 2) {
                    for (int group = curr; group < WholeModePlayoffOrdering.Count; group += 2) {
                        string group1 = WholeModePlayoffOrdering[group].Item2.Item1 + "/" + WholeModePlayoffOrdering[group].Item2.Item2;
                        string group2 = WholeModePlayoffOrdering[group+1].Item2.Item1 + "/" + WholeModePlayoffOrdering[group+1].Item2.Item2;
                        WholeModePlayoffOrdering.Add(Tuple.Create(round, Tuple.Create(group1, group2)));
                    }
                    curr = WholeModePlayoffOrdering.Count;
                    size /= 2;
                }
            }
        }
        else {
            if (randomInitialMode) {
                int second_round_bye = Convert.ToInt32(division["second_round_bye"]);

                int first_round_players = Convert.ToInt32(division["first_round_players"]);

                List<int> nonByeTeams = Enumerable.Range(second_round_bye+1, first_round_players * 2).ToList();

                List<List<int>> nonByeGroups = RandomlyGroup(nonByeTeams);

                int round_num = 1;

                for (int i = 0; i < nonByeGroups.Count; ++i) {
                    WholeModePlayoffOrdering.Add(Tuple.Create(round_num, Tuple.Create(nonByeGroups[i][0].ToString(), nonByeGroups[i][0].ToString())));
                }

                var taken = new Dictionary<int, string>();
                for (int i = 0; i < second_round_bye + first_round_players; ++i) {
                    taken[i+1] = "";
                }

                var random = new Random();
                int ct = 0;
                List<int> total_spots = Enumerable.Range(1, second_round_bye + first_round_players).ToList();
                while (ct < nonByeGroups.Count) {
                    var random_spot = total_spots[random.Next(0, total_spots.Count-1)];
                    if (taken.ContainsKey(random_spot)) {
                        continue;
                    }
                    taken[random_spot] = "BYE";
                    ct++;
                }

                List<int> allEmptySpots = new List<int>();

                foreach (var spot in taken) {
                    if (spot.Value == "") {
                        allEmptySpots.Add(spot.Key);
                    }
                }

                random = new Random();
                for (int i = 1; i <= first_round_players; ++i) {
                    var selection = allEmptySpots[random.Next(0, allEmptySpots.Count-1)];
                    allEmptySpots.Remove(selection);
                    taken[selection] = selection.ToString();
                }

                random = new Random();

                for (int i = 0; i < taken.Count; i += 2) {
                    var currPair = taken.ElementAt(i);
                    var first = "";
                    var second = "";
                    if (currPair.Value == "BYE") {
                        var matchupSelection = nonByeGroups[random.Next(0, nonByeGroups.Count-1)];
                        nonByeGroups.Remove(matchupSelection);
                        int found = 0;
                        while (matchupSelection[0] != nonByeGroups[found][0] && matchupSelection[1] != nonByeGroups[found][1]) {
                            found++;
                        }
                        first = "ROUND1INDEX" + found;
                    }
                    else {
                        first = currPair.Value.ToString();
                    }
                    var currPair2 = taken.ElementAt(i+1);
                    if (currPair2.Value == "BYE") {
                        var matchupSelection2 = nonByeGroups[random.Next(0, nonByeGroups.Count-1)];
                        nonByeGroups.Remove(matchupSelection2);
                        int found2 = 0;
                        while (matchupSelection2[0] != nonByeGroups[found2][0] && matchupSelection2[1] != nonByeGroups[found2][1]) {
                            found2++;
                        }
                        second = "ROUND1INDEX" + found2;
                    }
                    else {
                        second = currPair2.Value.ToString();
                    }

                    var newPair = Tuple.Create(first, second);

                    WholeModePlayoffOrdering.Add(Tuple.Create(2, newPair));
                }
            }
            else {
                foreach (var matchup in (List<Tuple<string, string>>) division["Round1"]) {
                    WholeModePlayoffOrdering.Add(Tuple.Create(1, Tuple.Create(matchup.Item1, matchup.Item2)));
                }

                foreach (var matchup in (List<Tuple<string, string>>) division["Round2"]) {
                    var player1 = matchup.Item1;
                    var player2 = matchup.Item2;
                    if (matchup.Item1.Contains("BYE")) {
                        player1 = "ROUND1INDEX" + matchup.Item1.Substring(3); 
                    }
                    if (matchup.Item2.Contains("BYE")) {
                        player2 = "ROUND1INDEX" + matchup.Item2.Substring(3); 
                    }

                    WholeModePlayoffOrdering.Add(Tuple.Create(2, Tuple.Create(player1, player2)));
                }
            }

            var round_number = 2;

            int group_num_size = 0;
            for (int i = 0; i < WholeModePlayoffOrdering.Count; ++i) {
                if (WholeModePlayoffOrdering[i].Item1 == 2) {
                    group_num_size++;
                }
            }

            if (randomRoundMode) {
                int pos = 0;
                while (group_num_size >= 2) {
                    List<string> tupleGroups = new List<string>();
                    for (int i = pos; i < WholeModePlayoffOrdering.Count; ++i) {
                        string matchupPlayers = WholeModePlayoffOrdering[i].Item2.Item1 + "/" + WholeModePlayoffOrdering[i].Item2.Item2;
                        tupleGroups.Add(matchupPlayers);
                    }
                    pos = WholeModePlayoffOrdering.Count;
                    List<List<string>> pickedGroups = RandomlyGroupTuples(tupleGroups);
                    foreach (var group in pickedGroups) {
                        WholeModePlayoffOrdering.Add(Tuple.Create(round_number, Tuple.Create(group[0], group[1])));
                    }
                    group_num_size /= 2;
                }
            }
            else {
                int size = group_num_size;
                int curr = 0;
                while (size >= 2) {
                    for (int group = curr; group < WholeModePlayoffOrdering.Count; group += 2) {
                        string group1 = WholeModePlayoffOrdering[group].Item2.Item1 + "/" + WholeModePlayoffOrdering[group].Item2.Item2;
                        string group2 = WholeModePlayoffOrdering[group+1].Item2.Item1 + "/" + WholeModePlayoffOrdering[group+1].Item2.Item2;
                        WholeModePlayoffOrdering.Add(Tuple.Create(round_number, Tuple.Create(group1, group2)));
                    }
                    curr = WholeModePlayoffOrdering.Count;
                    size /= 2;
                }
            }
        }

        return WholeModePlayoffOrdering;
    }

    [HttpPost("{LeaguePlayoffId}/DivisionbasedBracketRandom")]
    public async Task<ActionResult> GenerateRandomDivisionBasedBracket(string LeaguePlayoffId, Dictionary<string, object> reqBody) {
        try {
            var playoffs = (LeaguePlayoffs) await _leagueService.GetData("leaguePlayoffConfig", LeaguePlayoffId);
            if (playoffs == null) {
                return BadRequest();
            }

            int group_num = Convert.ToInt32(reqBody["num_of_players"]);

            reqBody.Remove("num_of_players");

            string divisionMode = reqBody["mode"].ToString() ?? String.Empty;

            reqBody.Remove("mode");

            List<Tuple<string, List<Tuple<int, Tuple<string, string>>>>> fullBracket = new List<Tuple<string, List<Tuple<int, Tuple<string, string>>>>>();

            foreach (var division in reqBody) {
                List<Tuple<int, Tuple<string, string>>> divisionBracket = RandomGenerateDivsionBasedBracket(playoffs.DefaultMode, playoffs.RandomInitialMode, playoffs.RandomRoundMode, group_num / reqBody.Keys.Count, (Dictionary<string, object>) division.Value);

                fullBracket.Add(Tuple.Create(division.Key, divisionBracket));
            }

            Dictionary<string, bool> upsertOpt = new Dictionary<string, bool>();
            upsertOpt[divisionMode] = false;
            Dictionary<string, object> updatedData = new Dictionary<string, object>();
            updatedData[divisionMode] = fullBracket;

            await _leagueService.EditData("leaguePlayoffConfig", upsertOpt, updatedData);

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("{LeaguePlayoffId}")]
    public async Task<ActionResult<Dictionary<string, object>>> UpdatePlayoffBracket(string LeaguePlayoffId, Dictionary<string, object> reqBody) {
        try {
            var playoffs = (LeaguePlayoffs) await _leagueService.GetData("leaguePlayoffConfig", LeaguePlayoffId);
            if (playoffs == null) {
                return BadRequest();
            }

            PlayoffBracket? tempBracket = playoffs.FinalPlayoffBracket;

            string playoffBracket = reqBody["bracket"].ToString() ?? String.Empty;

            int bracket = 0;

            if (playoffBracket != "") {
                for (int i = 0; i < tempBracket?.SubPlayoffBrackets.Count; ++i) {
                    if (playoffBracket == tempBracket?.SubPlayoffBrackets[i].PlayoffName) {
                        bracket = i;
                        break;
                    }
                }
            }

            List<Tuple<int, Tuple<string, string>>> Ordering = (List<Tuple<int, Tuple<string, string>>>) reqBody["Ordering"];

            string player1 = reqBody["player1"].ToString() ?? String.Empty; 
            string player2 = reqBody["player2"].ToString() ?? String.Empty;

            PlayoffGraphNode foundMatchup = tempBracket!.SubPlayoffBrackets[bracket].FindPlayerMatchup(player1, player2);

            foundMatchup.currentPlayoffMatchup.winner = reqBody["winner"].ToString() ?? String.Empty;

            Dictionary<string, object> resBody = new Dictionary<string, object>();
            
            if (foundMatchup.NextPlayoffMatch != null) {
                string rank = "";
                if (foundMatchup.currentPlayoffMatchup.winner == foundMatchup.currentPlayoffMatchup.player1) {
                    rank = foundMatchup.currentPlayoffMatchup.player1_rank.ToString();
                }
                if (foundMatchup.currentPlayoffMatchup.winner == foundMatchup.currentPlayoffMatchup.player2) {
                    rank = foundMatchup.currentPlayoffMatchup.player2_rank.ToString();
                }
                if (playoffs.DefaultMode) {
                    int index = 0;
                    for (int i = 0; i < Ordering.Count; ++i) {
                        if (Ordering[i].Item1 == foundMatchup.currentPlayoffMatchup.round + 1) {
                            if (Ordering[i].Item2.Item1.Contains(rank) || Ordering[i].Item2.Item2.Contains(rank)) {
                                PlayoffGraphNode node = tempBracket!.SubPlayoffBrackets[bracket].FindByPosition(Ordering[i].Item1, index);
                                node.currentPlayoffMatchup.PlayoffMatchupId = node.currentPlayoffMatchup.PlayoffMatchupId != "" ? node.currentPlayoffMatchup.PlayoffMatchupId : Guid.NewGuid().ToString();
                                node.currentPlayoffMatchup.round = node.currentPlayoffMatchup.round <= 0 ? node.currentPlayoffMatchup.round : Ordering[i].Item1;
                                node.currentPlayoffMatchup.winner = "";
                                if (node.currentPlayoffMatchup.player1 != "") {
                                    node.currentPlayoffMatchup.player2 = foundMatchup.currentPlayoffMatchup.winner;
                                    node.currentPlayoffMatchup.player2_rank = Convert.ToInt32(rank);
                                    node.currentPlayoffMatchup.series_player1_wins = 0;
                                }
                                else {
                                    node.currentPlayoffMatchup.player1 = foundMatchup.currentPlayoffMatchup.winner;
                                    node.currentPlayoffMatchup.player1_rank = Convert.ToInt32(rank);
                                    node.currentPlayoffMatchup.series_player2_wins = 0;
                                }
                                break;
                            }
                            index++;
                        } 
                    }
                }
                else {
                    if (foundMatchup.currentPlayoffMatchup.round == 1) {
                        var second_round_ordering = Ordering.GetRange(Ordering.IndexOf(Ordering.FirstOrDefault(t => t.Item1 == 2)!), Ordering.Count(tuple => tuple.Item1 == 2));
                        for (int i = 0; i < second_round_ordering.Count; ++i) {
                            int r = -1;
                            int ind = -1;
                            if (second_round_ordering[i].Item2.Item1.Contains("ROUND")) {
                                r = Convert.ToInt32(second_round_ordering[i].Item2.Item1.Substring(second_round_ordering[i].Item2.Item1.IndexOf("ROUND") + 1, 1));
                                ind = Convert.ToInt32(second_round_ordering[i].Item2.Item1.Substring(second_round_ordering[i].Item2.Item1.IndexOf("INDEX") + 1, 1));
                            }
                            else if (second_round_ordering[i].Item2.Item2.Contains("ROUND")) {
                                r = Convert.ToInt32(second_round_ordering[i].Item2.Item1.Substring(second_round_ordering[i].Item2.Item1.IndexOf("ROUND") + 1, 1));
                                ind = Convert.ToInt32(second_round_ordering[i].Item2.Item1.Substring(second_round_ordering[i].Item2.Item1.IndexOf("INDEX") + 1, 1)); 
                            }
                            if (r == -1 && ind == -1) {
                                continue;
                            }
                            PlayoffGraphNode node = tempBracket!.SubPlayoffBrackets[bracket].FindByPosition(r, ind);
                            if (node.currentPlayoffMatchup.player2_rank == Convert.ToInt32(rank) || node.currentPlayoffMatchup.player1_rank ==  Convert.ToInt32(rank)) {
                                if (node.NextPlayoffMatch?.currentPlayoffMatchup.player1 != null) {
                                    node.NextPlayoffMatch.currentPlayoffMatchup.player2 = foundMatchup.currentPlayoffMatchup.winner;  
                                    node.NextPlayoffMatch.currentPlayoffMatchup.player2_rank = foundMatchup.currentPlayoffMatchup.player2_rank;
                                    node.currentPlayoffMatchup.series_player2_wins = 0;
                                }
                                else {
                                    node.NextPlayoffMatch.currentPlayoffMatchup.player1 = foundMatchup.currentPlayoffMatchup.winner;  
                                    node.NextPlayoffMatch.currentPlayoffMatchup.player1_rank = foundMatchup.currentPlayoffMatchup.player1_rank;  
                                    node.currentPlayoffMatchup.series_player1_wins = 0;
                                }
                                if (node.NextPlayoffMatch.currentPlayoffMatchup.PlayoffMatchupId == null) {
                                    node.NextPlayoffMatch.currentPlayoffMatchup.PlayoffMatchupId = Guid.NewGuid().ToString();
                                }
                                node.currentPlayoffMatchup.round = second_round_ordering[i].Item1;

                                string matched = "ROUND" + r + "INDEX" + ind;
                                var tempOrdering = Ordering;
                                for (int j = 0; j < Ordering.Count; ++j) {
                                    if (Ordering[j].Item2.Item1.Contains(matched)) {
                                        if (Ordering[j].Item2.Item1.Contains("/")) {
                                            var arr = Ordering[j].Item2.Item1.Split("/").ToList();
                                            var ranks = new List<string>();
                                            foreach (var elt in arr) {
                                                if (elt == matched) {
                                                    ranks.Add(rank);
                                                }
                                                else {
                                                    ranks.Add(elt);
                                                }
                                            }

                                            Ordering[j] = Tuple.Create(Ordering[j].Item1, Tuple.Create(string.Join("/", ranks), Ordering[j].Item2.Item1));
                                        }
                                        else {
                                            Ordering[j] = Tuple.Create(Ordering[j].Item1, Tuple.Create(rank, Ordering[j].Item2.Item2));
                                        }
                                        
                                    }
                                    else if (Ordering[i].Item2.Item2.Contains(matched)) {
                                       if (Ordering[j].Item2.Item2.Contains("/")) {
                                            var arr = Ordering[j].Item2.Item2.Split("/").ToList();
                                            var ranks = new List<string>();
                                            foreach (var elt in arr) {
                                                if (elt == matched) {
                                                    ranks.Add(rank);
                                                }
                                                else {
                                                    ranks.Add(elt);
                                                }
                                            }

                                            Ordering[j] = Tuple.Create(Ordering[j].Item1, Tuple.Create(Ordering[j].Item2.Item1, string.Join("/", ranks)));
                                        }
                                        else {
                                            Ordering[j] = Tuple.Create(Ordering[j].Item1, Tuple.Create(Ordering[j].Item2.Item2, rank));
                                        }
                                    } 
                                }
                                break;
                            }
                        }
                    }
                    else {
                        var next_round_ordering = Ordering.GetRange(Ordering.IndexOf(Ordering.FirstOrDefault(t => t.Item1 == foundMatchup.currentPlayoffMatchup.round + 1)!), Ordering.Count(tuple => tuple.Item1 == foundMatchup.currentPlayoffMatchup.round + 1));
                        for (int j = 0; j < next_round_ordering.Count; ++j) {
                            int r = -1;
                            int ind = -1;
                            if (next_round_ordering[j].Item2.Item1.Contains(rank)) {
                                r = foundMatchup.currentPlayoffMatchup.round + 1;
                                ind = j;
                            }
                            else if (next_round_ordering[j].Item2.Item2.Contains(rank)) {
                                r = foundMatchup.currentPlayoffMatchup.round + 1;
                                ind = j;
                            }
                            if (r == -1 && ind == -1) {
                                continue;
                            }

                            PlayoffGraphNode foundNode = tempBracket!.SubPlayoffBrackets[bracket].FindByPosition(r, ind);
                            foundNode.currentPlayoffMatchup.PlayoffMatchupId = foundNode.currentPlayoffMatchup.PlayoffMatchupId != "" ? foundNode.currentPlayoffMatchup.PlayoffMatchupId : Guid.NewGuid().ToString();
                            if (foundNode.currentPlayoffMatchup.player1 != null) {
                                foundNode.currentPlayoffMatchup.player2 = foundMatchup.currentPlayoffMatchup.winner; 
                                foundNode.currentPlayoffMatchup.player2_rank = foundMatchup.currentPlayoffMatchup.player2_rank; 
                                foundNode.currentPlayoffMatchup.series_player2_wins = foundMatchup.currentPlayoffMatchup.series_player2_wins; 
                            }
                            else {
                                foundNode.currentPlayoffMatchup.player1 = foundMatchup.currentPlayoffMatchup.winner; 
                                foundNode.currentPlayoffMatchup.player1_rank = foundMatchup.currentPlayoffMatchup.player1_rank; 
                                foundNode.currentPlayoffMatchup.series_player1_wins = foundMatchup.currentPlayoffMatchup.series_player1_wins; 
                            }
                            foundNode.currentPlayoffMatchup.round = foundMatchup.currentPlayoffMatchup.round + 1;
                            break;
                        }
                    }
                }
            }
            else {
                if (tempBracket.SubPlayoffBrackets.Count == 1) {
                    resBody["SeasonComplete"] = true;
                    resBody["Champion"] = reqBody["winner"].ToString() ?? String.Empty;
                }
                else {
                    resBody["SeasonComplete"] = false;
                    resBody["finalRoundPlayer"] = reqBody["winner"].ToString() ?? String.Empty;
                }
            }

            Dictionary<string, bool> upsertOpt = new Dictionary<string, bool>();
            upsertOpt["FinalFullBracket"] = false;
            upsertOpt["WholeRoundOrdering"] = false;
            Dictionary<string, object> updatedData = new Dictionary<string, object>();
            updatedData["FinalFullBracket"] = tempBracket;
            updatedData["WholeRoundOrdering"] = Ordering;

            await _leagueService.EditData("leaguePlayoffConfig", upsertOpt, updatedData);

            OkObjectResult res = new OkObjectResult(resBody);
            
            return Ok(res);
        } catch {
            return BadRequest();
        }
    }

    [HttpGet("{LeaguePlayoffId}/{PlayerA}/{PlayerB}/{mode}/{playoffBracket}")]
    public async Task<ActionResult<PlayoffMatchup>> GetPlayoffMatchupFromBracket(string LeaguePlayoffId, String PlayerA, String PlayerB, String playoffBracket) {
        try {
            var playoffs = (LeaguePlayoffs) await _leagueService.GetData("leaguePlayoffConfig", LeaguePlayoffId);
            if (playoffs == null) {
                return BadRequest();
            }

            int bracket = 0;

            if (playoffBracket != "") {
                for (int i = 0; i < playoffs.FinalPlayoffBracket?.SubPlayoffBrackets.Count; ++i) {
                    if (playoffBracket == playoffs.FinalPlayoffBracket?.SubPlayoffBrackets[i].PlayoffName) {
                        bracket = i;
                        break;
                    }
                }
            }

            PlayoffGraphNode? foundNode = playoffs.FinalPlayoffBracket?.SubPlayoffBrackets[bracket].FindPlayerMatchup(PlayerA, PlayerB);

            OkObjectResult res = new OkObjectResult(foundNode?.currentPlayoffMatchup);

            return Ok(res);
        } catch {
            return BadRequest();
        }
    }

    [HttpGet("{LeaguePlayoffId}/PlayoffBracket")]
    public async Task<ActionResult<PlayoffBracket>> GetPlayoffBracket(string LeaguePlayoffId) {
        var playoffs = (LeaguePlayoffs) await _leagueService.GetData("leaguePlayoffConfig", LeaguePlayoffId);
        if (playoffs == null) {
            return BadRequest();
        }

        OkObjectResult res = new OkObjectResult(playoffs.FinalPlayoffBracket);
        return Ok(res);
    }

    [HttpPut("{LeaguePlayoffId}/PlayoffMatchup")]
    public async Task<ActionResult> AddPlayoffMatchupDate(string LeaguePlayoffId, Dictionary<string, string> reqBody) {
        try {
            var playoffs = (LeaguePlayoffs) await _leagueService.GetData("leaguePlayoffConfig", LeaguePlayoffId);
            if (playoffs == null) {
                return BadRequest();
            }

            string player1 = reqBody["player1"].ToString() ?? String.Empty; 
            string player2 = reqBody["player2"].ToString() ?? String.Empty;

            var leagueBracket = playoffs.FinalPlayoffBracket;

            string playoffBracket = reqBody["bracket"].ToString() ?? String.Empty;

            int bracket = 0;

            if (playoffBracket != "") {
                for (int i = 0; i < leagueBracket?.SubPlayoffBrackets.Count; ++i) {
                    if (playoffBracket == leagueBracket?.SubPlayoffBrackets[i].PlayoffName) {
                        bracket = i;
                        break;
                    }
                }
            }

            PlayoffGraphNode foundMatchup = leagueBracket!.SubPlayoffBrackets[bracket].FindPlayerMatchup(player1, player2);

            foundMatchup.currentPlayoffMatchup.GameId = reqBody["GameId"];

            Dictionary<string, bool> upsertOpt = new Dictionary<string, bool>();
            upsertOpt["FinalPlayoffBracket"] = false;

            Dictionary<string, object> updatedData = new Dictionary<string, object>();
            updatedData["FinalPlayoffBracket"] = leagueBracket;


            await _leagueService.EditData("leaguePlayoffConfig", upsertOpt, updatedData);

            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

}