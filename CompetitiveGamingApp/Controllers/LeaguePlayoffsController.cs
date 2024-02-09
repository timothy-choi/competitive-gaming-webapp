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
                                if (elt.Contains("BYE") && (!int.TryParse(elt.Substring(elt.IndexOf("BYE") + 3), out int result) || !(result > 0 && result <= playoffs["ROUND" + r].Count-1))) {
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
                            if (arr.Count > (r-1) || arr.Count < (r-1)) {
                                return false;
                            }
                            foreach (var elt in arr) {
                                if (!int.TryParse(elt, out _) || seen[elt]) {
                                    return false;
                                }
                                if (elt.Contains("BYE") && (!int.TryParse(elt.Substring(elt.IndexOf("BYE") + 3), out int result2) || !(result2 > 0 && result2 <= playoffs["ROUND" + r].Count-1)))
                                {
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
                            teamB = "ROUND:" + (complex_index-1) + "INDEX:" + matchup.Item2.Substring(3);
                        }
                        if (matchup.Item1.Contains("BYE")) {
                            teamA = teamA.Substring(0, teamA.IndexOf("BYE")) + "ROUND:" + (complex_index-1) + "INDEX:" + matchup.Item1.Substring(teamA.IndexOf("BYE") + 3);
                        }
                        if (matchup.Item2.Contains("BYE")) {
                            teamB = teamB.Substring(0, teamB.IndexOf("BYE")) + "ROUND:" + (complex_index-1) + "INDEX:" + matchup.Item2.Substring(teamB.IndexOf("BYE") + 3);
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
                            while (matchupSelection[0] != nonByeGroups[found][0] && matchupSelection[0] != nonByeGroups[found][1]) {
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
                            while (matchupSelection2[0] != nonByeGroups[found2][0] && matchupSelection2[0] != nonByeGroups[found2][1]) {
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
                    for (int i = 0; i < WholeModePlayoffOrdering.Count; ++i) {
                        if (WholeModePlayoffOrdering[i].Item1 == 2) {
                            group_num_size++;
                        }
                    }
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
}