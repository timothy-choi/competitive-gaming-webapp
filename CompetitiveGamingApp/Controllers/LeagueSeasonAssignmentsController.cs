namespace CompetitiveGamingApp.Controller;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using CompetitiveGamingApp;
using RabbitMQ;

[ApiController]
[Route("api/LeagueSeasonAssignments")]
public class LeagueSeasonAssignmentsController : ControllerBase {
    private readonly MongoDBService _leagueService;

    private readonly Producer _producer;
    public LeagueSeasonAssignmentsController(MongoDBService leagueService) {
        _leagueService = leagueService;
        _producer = new Producer();
    }

    [HttpGet("{AssignmentId}")]
    public async Task<ActionResult<LeaguePlayerSeasonAssignments>> GetSeasonAssignments(string AssignmentId) {
        var assignment = (LeaguePlayerSeasonAssignments) await _leagueService.GetData("leagueSeasonAssignments", AssignmentId);
        if (assignment == null) {
            return NotFound();
        }
        OkObjectResult res = new OkObjectResult(assignment);
        return Ok(res);
    }

    [HttpPost]
    public async Task<ActionResult<string>> CreateSeasonAssignments(Dictionary<string, object> reqBody) {
        try {
            LeaguePlayerSeasonAssignments currLeague = new LeaguePlayerSeasonAssignments {
                AssignmentsId = Guid.NewGuid().ToString(),
                ConfigId = reqBody["ConfigId"] as string,
                LeagueId = reqBody["LeagueId"] as string,
                PartitionsEnabled = Convert.ToBoolean(reqBody["PartitionsEnabled"]),
                ReassignEverySeason = Convert.ToBoolean(reqBody["ReassignEverySeason"]),
                AutomaticInduction = Convert.ToBoolean(reqBody["AutomaticInduction"]),
                NumberOfPlayersPerPartition = Convert.ToInt32(reqBody["NumberOfPlayersPerPartition"]),
                NumberOfPartitions = Convert.ToInt32(reqBody["NumberOfPartitions"]),
                SamePartitionSize = Convert.ToBoolean(reqBody["SamePartitionSize"]),
                AutomaticScheduling = Convert.ToBoolean(reqBody["AutomaticScheduling"]),
                ExcludeOutsideGames = Convert.ToBoolean(reqBody["ExcludeOutsideGames"]),
                InterDvisionGameLimit = Convert.ToInt32(reqBody["InterDivisionGameLimit"]),
                RepeatMatchups = Convert.ToBoolean(reqBody["RepeatMatchups"]),
                MaxRepeatMatchups = Convert.ToInt32(reqBody["MaxRepeatMatchups"]),
                DivisionSelective = Convert.ToBoolean(reqBody["DivisionSelective"]),
                OutsideDivisionSelections = new Dictionary<string, List<string>>(),
                RandomizeDivisionSelections = Convert.ToBoolean(reqBody["RandomizeDivisionSelections"]),
                PlayerSelection = Convert.ToBoolean(reqBody["PlayerSelection"]),
                PlayerExemptLists = new Dictionary<string, List<string>>(),
                repeatAllMatchups = Convert.ToBoolean(reqBody["repeatAllMatchups"]),
                minRepeatMatchups = Convert.ToInt32(reqBody["minRepeatMatchups"]),
                maxRepeatMatchups = Convert.ToInt32(reqBody["maxRepeatMatchups"]),
                playAllPlayers = Convert.ToBoolean(reqBody["playAllPlayers"]),
                AllPartitions = new Dictionary<String, List<String>>(),
                AllCombinedDivisions = new Dictionary<String, List<String>>(),
                PlayerFullSchedule = new List<Tuple<string, List<object>>>(),
                ArchievePlayerFullSchedule = new List<List<Tuple<string, List<object>>>>(),
                FinalFullSchedule = new List<SingleGame>(),
                ArchieveFinalFullSchedule = new List<List<SingleGame>>()
            };

            await _leagueService.PostData("leagueSeasonAssignments", currLeague);

            OkObjectResult res = new OkObjectResult(currLeague.AssignmentsId);
            return Ok(res);
        } catch {
            return BadRequest();      
        }
    }

    [HttpDelete("{AssignmentsId}")]
    public async Task<ActionResult> DeleteSeasonAssignments(string AssignmentsId) {
        try {
            var assignment = _leagueService.GetData("leagueSeasonAssignments", AssignmentsId);
            if (assignment == null) {
                return NotFound();
            }

            await _leagueService.DeleteData("leagueSeasonAssignments", AssignmentsId);
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("{AssignmentsId}")]
    public async Task<ActionResult> EditSeasonAssignmentsOptions(string AssignmentsId, Dictionary<string, object> reqBody) {
        try {
            var assignment = _leagueService.GetData("leagueSeasonAssignments", AssignmentsId);
            if (assignment == null) {
                return NotFound();
            }

            Dictionary<string, bool> upsertInfo = new Dictionary<string, bool>();
            Dictionary<string, object> updatedValues = new Dictionary<string, object>();

            foreach (var setting in reqBody) {
                if (reqBody[setting.Key] is Tuple<bool, object> tupleValue)
                {
                    upsertInfo[setting.Key] = tupleValue.Item1;
                    updatedValues[setting.Key] = tupleValue.Item2;
                }
            }

            await _leagueService.EditData("leagueSeasonAssignments", upsertInfo, updatedValues);

            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    [HttpPut("{AssignmentsId}/OutsideDivision")]
    public async Task<ActionResult> EditOutsideDivisionSelection(string AssignmentsId, Dictionary<string, object> reqBody) {
        try {
            var assignment = (LeaguePlayerSeasonAssignments) await _leagueService.GetData("leagueSeasonAssignments", AssignmentsId);
            if (assignment == null) {
                return NotFound();
            }

            Dictionary<string, bool> upsertInfo = new Dictionary<string, bool>();
            Dictionary<string, object> updatedValues = new Dictionary<string, object>();

            upsertInfo[(String) reqBody["indexName"]] = (bool) reqBody["upsertStatus"];

            if ((bool) reqBody["deleteOption"]) {
                var index = -1;
                var selections = assignment.OutsideDivisionSelections;
                for (int i = 0; i < selections.Count; ++i) {
                    if ((int) reqBody["selectedIndex"] == i) {
                        index = i;
                        break;
                    }
                }

                if (index == -1) {
                    return NotFound();
                }

                var key = selections.Keys.ElementAt(index);
                selections.Remove(key);
                updatedValues[(String) reqBody["indexName"]] = selections;
            }
            else {
                updatedValues[(String) reqBody["indexName"]] = reqBody["DivisionValues"];
            }

            await _leagueService.EditData("leagueSeasonAssignments", upsertInfo, updatedValues);
            return Ok();
        } catch {
            return BadRequest();
        }
    } 

    [HttpPost("{AssignmentsId}/DivisionSelections")]
    public async Task<ActionResult> GenerateDivisionSelectionsIndividual(string AssignmentsId, Dictionary<string, object> reqBody) {
        try {
            var assignment = (LeaguePlayerSeasonAssignments) await _leagueService.GetData("leagueSeasonAssignments", AssignmentsId);
            if (assignment == null) {
                return NotFound();
            }
            
            var divisionSelection = assignment.OutsideDivisionSelections;

            Dictionary<string, bool> upsertInfo = new Dictionary<string, bool>();
            upsertInfo["OutsideDivisionSelections"] = false;
            Dictionary<string, object> updatedValues = new Dictionary<string, object>();

            Dictionary<string, string> player_divisions = new Dictionary<string, string>();

            var players = (List<String>) reqBody["players"];
            var divisions = (List<String>) reqBody["divisions"];
            for (int x = 0; x < players.Count; ++x) {
                for (int y = 0; y < divisions.Count; ++y) {
                    var divisionAssignment = (Dictionary<string, List<string>>)reqBody["division_assignment"];
                    if (divisionAssignment.TryGetValue(divisions[y], out var divisionPlayers)) {
                        var foundIndex = divisionPlayers.FindIndex(player => player == players[x]);
                        if (foundIndex != -1) {
                            player_divisions[players[x]] = divisions[y];
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < players.Count; ++i) {
                var random = new Random();
                divisions =  (List<String>) reqBody["divisions"];
                 var player = (String) players[i]; 
                while (divisionSelection[player].Count != (int) reqBody["num_of_selections"]) {
                    var selection = divisions[random.Next(0, divisions.Count-1)];
                    divisionSelection[player].Add(selection);
                    divisions.RemoveAll(d => d == selection);

                    var divisionAssignment = ((Dictionary<string, List<string>>)reqBody["division_assignment"])[selection];

                    for (int j = 0; j < divisionAssignment.Count; ++j) {
                        divisionSelection[divisionAssignment[j]].Add(player_divisions[player]);
                    }
                }
            } 

            updatedValues["OutsideDivisionSelections"] = divisionSelection;

            await _leagueService.EditData("leagueSeasonAssignments", upsertInfo, updatedValues);
            return Ok();
        } catch {
            return BadRequest();
        }
    }
    [HttpPost("{AssignmentsId}/ExemptLists")]
    public async Task<ActionResult> AddExemptLists(string AssignmentsId, Dictionary<string, object> reqBody) {
        try {
            var assignment = _leagueService.GetData("leagueSeasonAssignments", AssignmentsId);
            if (assignment == null) {
                return NotFound();
            }

            Dictionary<string, bool> upsertInfo = new Dictionary<string, bool>();
            upsertInfo[(String) reqBody["player_list_key"]] = false;
            Dictionary<string, object> updatedValues = new Dictionary<string, object>();

            updatedValues[(String) reqBody["player_list_key"]] = reqBody["exempt_lists"];

            await _leagueService.EditData("leagueSeasonAssignments", upsertInfo, updatedValues);

            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    [HttpPut("{AssignmentsId}/ExemptLists")]
    public async Task<ActionResult> EditExemptList(string AssignmentsId, Dictionary<string, object> reqBody) {
        try {
            var assignment = (LeaguePlayerSeasonAssignments) await _leagueService.GetData("leagueSeasonAssignments", AssignmentsId);
            if (assignment == null) {
                return NotFound();
            }

            Dictionary<string, bool> upsertInfo = new Dictionary<string, bool>();

            Dictionary<string, object> updatedValues = new Dictionary<string, object>();

            foreach (var player in reqBody) {
                var updates = (List<Tuple<bool, string, int>>)player.Value;
                foreach (var update in updates) {
                    upsertInfo[update.Item2] = update.Item1 ? false : true;
                    if (!update.Item1) {
                        var exemptList = assignment.PlayerExemptLists[player.Key];

                        exemptList.RemoveAt(update.Item3);

                        updatedValues[update.Item2] = exemptList;
                    }
                    else {
                        updatedValues[update.Item2] = update.Item3;
                    }
                }
            }

            await _leagueService.EditData("leagueSeasonAssignments", upsertInfo, updatedValues);

            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    [HttpPost("{AssignmentsId}/Divisions")]
    public async Task<ActionResult> AddDivisions(string AssignmentsId, Dictionary<string, object> reqBody) {
        try {
            var assignment = (LeaguePlayerSeasonAssignments) await _leagueService.GetData("leagueSeasonAssignments", AssignmentsId);
            if (assignment == null) {
                return NotFound();
            }

            var count = 0;

            Dictionary<string, bool> upsertInfo = new Dictionary<string, bool>();
            upsertInfo["AllPartitions"] = false;

            Dictionary<string, object> updatedValues = new Dictionary<string, object>();

            var partitions = assignment.AllPartitions;

            var seen = new List<string>();
            foreach (var division in reqBody) {
                partitions[division.Key] = (List<String>) reqBody[division.Key];
                foreach (var player in (List<String>) reqBody[division.Key]) {
                    seen.Add(player);
                    count++;
                }
            }

            if (seen.Distinct().Count() != count) {
                return BadRequest();
            }

            updatedValues["AllPartitions"] = partitions;

            await _leagueService.EditData("leagueSeasonAssignments", upsertInfo, updatedValues);

            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    [HttpPost("{AssignmentsId}/GenerateDivision")]
    public async Task<ActionResult> GenerateDivisions(string AssignmentsId, Dictionary<string, object> reqBody) {
        try {
            var assignment = (LeaguePlayerSeasonAssignments) await _leagueService.GetData("leagueSeasonAssignments", AssignmentsId);
            if (assignment == null) {
                return NotFound();
            }

            Dictionary<string, bool> upsertInfo = new Dictionary<string, bool>();
            upsertInfo["AllPartitions"] = false;

            Dictionary<string, object> updatedValues = new Dictionary<string, object>();

            var partitions = assignment.AllPartitions;

            var players = (List<String>) reqBody["players"];

            var min_num_players_on_group = (int) reqBody["num_num_players_per_group"];

            var random = new Random();

            foreach (var division in (List<String>) reqBody["divisions"]) {
                for (int i = 0; i < min_num_players_on_group; ++i) {
                    var selection = players[random.Next(0, players.Count-1)];
                    players.RemoveAll(player => player == selection);
                    partitions[division].Add(selection);
                }
            }

            if (players.Count > 0) {
                random = new Random();
                var divisions = (List<String>) reqBody["divisions"];
                while (players.Count > 0) {
                    var div_selection = divisions[random.Next(0, divisions.Count-1)];
                    var player_selection = players[random.Next(0, players.Count-1)];
                    players.RemoveAll(player => player == player_selection);

                    partitions[div_selection].Add(player_selection);
                }
            }

            updatedValues["AllPartitions"] = partitions;

            await _leagueService.EditData("leagueSeasonAssignments", upsertInfo, updatedValues);

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("{AssignmentsId}/EditDivisions")]
    public async Task<ActionResult> EditDivisions(string AssignmentsId, Dictionary<string, object> reqBody) {
        try {
            var assignment = (LeaguePlayerSeasonAssignments) await _leagueService.GetData("leagueSeasonAssignments", AssignmentsId);
            if (assignment == null) {
                return NotFound();
            }

            Dictionary<string, bool> upsertInfo = new Dictionary<string, bool>();
            upsertInfo["AllPartitions"] = (bool) reqBody["remove"];

            Dictionary<string, object> updatedValues = new Dictionary<string, object>();

            Dictionary<string, bool> resBody = new Dictionary<string, bool>();

            if (!upsertInfo["AllPartitions"]) {
                var divisionAssignment = assignment.AllPartitions[(String) reqBody["target_division"]];
                divisionAssignment.RemoveAll(player => player == reqBody["selected_player"]);
                updatedValues["AllPartitions"] = divisionAssignment;
                if (divisionAssignment.Count < (int) reqBody["min_num_of_players_in_division"]) {
                    resBody["change"] = true;
                }
                else {
                    resBody["change"] = false;
                }
            }
            else {
                updatedValues[(String) reqBody["division_key"]] = reqBody["new_player"];  
            }

            await _leagueService.EditData("leagueSeasonAssignments", upsertInfo, updatedValues);

            OkObjectResult res = new OkObjectResult(resBody);

            return Ok(res);
        } catch {
            return BadRequest();
        }
    }

    [HttpGet("{AssignmentsId}/Divisions")]
    public async Task<ActionResult<Dictionary<string, List<string>>>> GetCurrentDivisionSelections(string AssignmentsId) {
        var assignment = (LeaguePlayerSeasonAssignments) await _leagueService.GetData("leagueSeasonAssignments", AssignmentsId);
        if (assignment == null) {
            return NotFound();
        }

        OkObjectResult res = new OkObjectResult(assignment.AllPartitions);

        return Ok(res);
    }

    [HttpPost("{AssignmentsId}/Combined")]
    public async Task<ActionResult> AddCombinedDivisionSelections(string AssignmentsId, Dictionary<string, object> reqBody) {
        try {
            var assignment = (LeaguePlayerSeasonAssignments) await _leagueService.GetData("leagueSeasonAssignments", AssignmentsId);
            if (assignment == null) {
                return NotFound();
            }

            Dictionary<string, bool> upsertInfo = new Dictionary<string, bool>();
            upsertInfo["AllCombinedDivisions"] = false;

            Dictionary<string, object> updatedValues = new Dictionary<string, object>();

            var combinedDivisions = assignment.AllCombinedDivisions;

            foreach (var combDivision in reqBody) {
                combinedDivisions[combDivision.Key] = (List<String>) reqBody[combDivision.Key];
            }

            updatedValues["AllCombinedDivisions"] = combinedDivisions;

            await _leagueService.EditData("leagueSeasonAssignments", upsertInfo, updatedValues);

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    private bool verifySchedule(Dictionary<string, List<List<string>>> schedule, List<string> players) {
        foreach (var player in schedule) {
            List<string> schedule_players = schedule[player.Key].Select(p => p.ElementAtOrDefault(0)).Distinct().ToList();
            var player_ref = players.Where(p => p != player.Key).Distinct().ToList();

            if (schedule_players.Intersect(player_ref).ToList().Count() != schedule_players.Count()) {
                return false;
            }
        }

        foreach (var player in schedule) {
            for (int index = 0; index < schedule[player.Key].Count; ++index) {
                var opponent = schedule[player.Key][index][0];
                if (schedule[opponent][index][0] != player.Key) {
                    return false;
                }
                if (schedule[opponent][index][2] == "H" && schedule[player.Key][index][2] == "H" || schedule[opponent][index][2] == "A" && schedule[player.Key][index][2] == "A") {
                    return false;
                }
            }
        }

        foreach (var player in schedule) {
            for (int index = 0; index < schedule[player.Key].Count - 1; ++index) {
                int compareDates = schedule[player.Key][index][1].CompareTo(schedule[player.Key][index+1][1]);
                if (compareDates >= 0) {
                    return false;
                }
            }
        }

        return true;
    }

    private Dictionary<string, List<List<string>>> ParseSchedule(string scheduleContent) {
        var schedule = new Dictionary<string, List<List<string>>>();
        List<List<string>> currentSchedule = null;

        using (var streamReader = new StreamReader(scheduleContent)) {
            string line;
            while ((line = streamReader.ReadLine()) != null) {
                if (char.IsDigit(line[0])) {
                    var player_schedule = line.Substring(3).Trim();
                    currentSchedule = new List<List<string>>();
                    schedule[player_schedule] = currentSchedule;
                }
                else if (currentSchedule != null) {
                    List<string> gameInfo = line.Split(',').ToList();
                    currentSchedule.Add(gameInfo);
                }
            }
        }

        return schedule;
    }

    private Dictionary<string, List<List<object>>> SimplifySchedule(Dictionary<string, List<List<object>>> schedule) {
        Dictionary<string, List<List<object>>> final_schedule = new Dictionary<string, List<List<object>>>();
        int index = 0;
        foreach (var player in schedule) {
            final_schedule[player.Key] = new List<List<object>>(schedule[player.Key].Count);
        }

        foreach (var player in schedule) {
            for (int i = 0; i < schedule[player.Key].Count; ++i) {
                if (schedule[player.Key][i].Count < 3) {
                    continue;
                }
                final_schedule[player.Key][i] = schedule[player.Key][i];
                final_schedule[(String) schedule[player.Key][i][0]][i] = new List<object> { (object)index, (object)i }; 
            }
            index++;
        }

        return final_schedule;
    }

    //Endpoint to recieve a file of all schedules and verify it -> add it to file
    [HttpPost("{AssignmentsId}/UploadSchedule")]
    public async Task<ActionResult<List<Tuple<string, List<List<object>>>>>> ProcessSubmittedSchedule(string AssignmentsId, [FromForm] IFormFile schedule, [FromBody] Dictionary<string, object> reqBody) {
        try {
            if (schedule == null || schedule.Length == 0) {
                return BadRequest();
            }

            Dictionary<string, List<List<string>>> player_schedules = new Dictionary<string, List<List<string>>>();

            List<Tuple<string, List<List<object>>>> final_player_schedule = new List<Tuple<string, List<List<object>>>>();

            using (var streamReader = new StreamReader(schedule.OpenReadStream())) {
                var fileContent = await streamReader.ReadToEndAsync();

                player_schedules = ParseSchedule(fileContent);

                var verified = verifySchedule(player_schedules, (List<String>) reqBody["players"]);

                if (!verified) {
                    return BadRequest();
                }

                Dictionary<string, List<List<object>>> temp = new Dictionary<string, List<List<object>>>();
                foreach (var entry in player_schedules) {
                    var newList = entry.Value.Select(innerList => innerList.Cast<object>().ToList()).ToList();
                    temp.Add(entry.Key, newList);
                }

                Dictionary<string, List<List<object>>> final_schedule = SimplifySchedule(temp);

                final_player_schedule = new List<Tuple<string, List<List<object>>>>();

                foreach (var player in final_schedule) {
                    final_player_schedule.Add(Tuple.Create(player.Key, final_schedule[player.Key]));
                }

            }

            Dictionary<string, object> resBody = new Dictionary<string, object>();
            resBody["PlayerFullSchedule"] = final_player_schedule;

            OkObjectResult res = new OkObjectResult(resBody);

            return Ok(res);
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("{AssignmentsId}/SendProcessScheduleMQ")]
    public async Task<ActionResult> SendSubmittedScheduleToMQ(string AssignmentsId, Dictionary<string, object> reqBody) {
        var assignment = (LeaguePlayerSeasonAssignments) await _leagueService.GetData("leagueSeasonAssignments", AssignmentsId);
        if (assignment == null) {
            return NotFound();
        }

        reqBody["assignmentsId"] = AssignmentsId;
        _producer.SendProcessSubmittedScheduleMessage(reqBody);

        return Ok();
    }

    //Endpoint to notify SNS/other MQ with request to generate schedules. 
    [HttpPost("{AssignmentsId}")]
    public async Task<ActionResult<List<Tuple<string, List<object>>>>> GeneratePlayerSchedules(string AssignmentsId, Dictionary<string, object> reqBody) {
        try {
            var assignment = (LeaguePlayerSeasonAssignments) await _leagueService.GetData("leagueSeasonAssignments", AssignmentsId);
            if (assignment == null) {
                return NotFound();
            }

            Dictionary<string, object> payload = new Dictionary<string, object>();
            payload["event_type"] = "send_message"; 
            payload["whole_mode"] = reqBody["WholeMode"];
            payload["playAllTeams"] = assignment.playAllPlayers;
            payload["players"] = (List<string>) reqBody["players"];
            payload["num_games"] = Convert.ToInt32(reqBody["num_games"]);
            payload["min_repeat_times"] = assignment.minRepeatMatchups;
            payload["max_repeat_times"] = assignment.maxRepeatMatchups;
            payload["start_dates"] = (Dictionary<string, DateTime>) reqBody["start_dates"];
            payload["intervals_between_games"] = Convert.ToInt32(reqBody["intervals_between_games"]);
            payload["intervals_between_games_hours"] = Convert.ToInt32(reqBody["intervals_between_games_hours"]);
            payload["do_not_play"] = reqBody.ContainsKey("do_not_play") ? (Dictionary<string, List<string>>) reqBody["do_not_play"] : new Dictionary<string, List<string>>();
            payload["groups"] =   reqBody.ContainsKey("groups") ? (Dictionary<string, List<string>>) reqBody["groups"] : new Dictionary<string, List<string>>();
            payload["outside_groups"] = reqBody.ContainsKey("outside_groups")  ? (Dictionary<string, List<string>>) reqBody["outside_groups"] : new Dictionary<string, List<string>>();
            payload["player_groups"] = reqBody.ContainsKey("player_groups") ? (Dictionary<string, List<string>>) reqBody["player_groups"] : new Dictionary<string, List<string>>();
            payload["outside_player_limit"] = reqBody.ContainsKey("outside_player_limit") ? Convert.ToInt32(reqBody["outside_player_limit"]) : 0;
            payload["max_repeat_outside_matches"] = assignment.MaxRepeatMatchups;
            payload["exclude_outside_divisions"] = assignment.ExcludeOutsideGames;
            payload["repeat_matchups"] = assignment.RepeatMatchups;

            var lambdaConfig = new AmazonLambdaConfig
            {
                RegionEndpoint = RegionEndpoint.USEast1,
            };

            using var lambdaClient = new AmazonLambdaClient(lambdaConfig);

            var request = new InvokeRequest
            {
                FunctionName = "ScheduleHandler",
                Payload = JsonConvert.SerializeObject(payload)
            };

            var response = await lambdaClient.InvokeAsync(request);

            if (response.HttpStatusCode != HttpStatusCode.OK) {
                return BadRequest();
            }

            Dictionary<string, List<object>>? dictionary = JsonConvert.DeserializeObject<Dictionary<string, List<object>>>(Encoding.UTF8.GetString(response.Payload.ToArray()));

            List<Tuple<string, List<object>>> tupleList = new List<Tuple<string, List<object>>>();
            foreach (var kvp in dictionary!)
            {
                tupleList.Add(new Tuple<string, List<object>>(kvp.Key, kvp.Value));
            }

            OkObjectResult res = new OkObjectResult(tupleList);

            return Ok(res);
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("{AssignmentsId}/GenerateScheduleMQ")]
    public async Task<ActionResult> AddGenerateScheduleRequestToQueue(string AssignmentsId, Dictionary<string, object> reqBody) {
        var assignment = (LeaguePlayerSeasonAssignments) await _leagueService.GetData("leagueSeasonAssignments", AssignmentsId);
        if (assignment == null) {
            return NotFound();
        }

        reqBody["assignmentsId"] = AssignmentsId;
        _producer.SendGenerateScheduleMessage(reqBody);
        return Ok();
    }

    //Endpoint to take in generated schedules and format it using the SingleGame Objects for each player's schedule
    [HttpPut("{AssignmentsId}")]    
    public async Task<ActionResult> ProcessPlayerSchedules(string AssignmentsId, Dictionary<string, object> reqBody) {
        try {
            var assignment = (LeaguePlayerSeasonAssignments) await _leagueService.GetData("leagueSeasonAssignments", AssignmentsId);
            if (assignment == null) {
                return NotFound();
            }

            Dictionary<string, bool> upsertInfo = new Dictionary<string, bool>();
            upsertInfo["PlayerFullSchedule"] = false;

            Dictionary<string, object> updatedValues = new Dictionary<string, object>();

            var playerSchedules = assignment.PlayerFullSchedule;

            var schedules = (List<Tuple<string, List<object>>>) reqBody["PlayerFullSchedule"];

            var size = schedules[0].Item2.Count;

            foreach (var schedule in schedules) {
                List<object> temp = new List<object>(size);
                playerSchedules.Add(Tuple.Create(schedule.Item1, temp));
            }

            int index1 = 0;

            foreach (var schedule in schedules) {
                int index2 = 0;
                foreach (var gameInfo in schedule.Item2) {
                    Type currType = gameInfo.GetType();
                    if (!currType.IsClass  && !currType.IsValueType) {
                        continue;
                    }
                    SingleGame currGame = new SingleGame {
                        SingleGameId = Guid.NewGuid().ToString(),
                        hostPlayer = (string)((List<object>)gameInfo)[2] == "H" ? schedule.Item1 : (string)((List<object>)gameInfo)[0],
                        guestPlayer = (string)((List<object>)gameInfo)[2] == "A" ? schedule.Item1 : (string)((List<object>)gameInfo)[0],
                        finalScore = new Tuple<int, int>(0, 0),
                        inGameScores = new List<Tuple<String, Tuple<int, int>>>(),
                        timePlayed = (DateTime)((List<object>)gameInfo)[1],
                        videoObjName = "",
                        gameEditor = "",
                        twitchBroadcasterId = "",
                        otherGameInfo = new Dictionary<String, String>()
                    };
                    int found_index = 0;
                    foreach (var player in schedules) {
                        if (player.Item1 == (string)((List<object>)gameInfo)[0]) {
                            break;
                        }
                        found_index++;
                    }
                    playerSchedules[index1].Item2[index2] = currGame;
                    playerSchedules[found_index].Item2[index2] = new List<int> { index1, index2 };

                    using (HttpClient client = new HttpClient()) {
                        
                        var jsonData = JsonConvert.SerializeObject(currGame);

                        StringContent resBody = new StringContent(jsonData, Encoding.UTF8, "application/json");
                        HttpResponseMessage resMessage = await client.PostAsync("/api/singleGame/Season", resBody);

                        if (!resMessage.IsSuccessStatusCode) {
                            return BadRequest();
                        }
                    }
                    index2++;
                }
                index1++;
            }

            updatedValues["PlayerFullSchedule"] = playerSchedules;

            await _leagueService.EditData("leagueSeasonAssignments", upsertInfo, updatedValues);

            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    [HttpPost("{AssignmentsId}/ProcessedSchedulesMQ")]
    public async Task<ActionResult> AddToProcessedSchedulesMQ(string AssignmentsId, Dictionary<string, object> reqBody) {
        var assignment = (LeaguePlayerSeasonAssignments) await _leagueService.GetData("leagueSeasonAssignments", AssignmentsId);
        if (assignment == null) {
            return NotFound();
        }

        reqBody["AssignmentsId"] = AssignmentsId;
        _producer.SendProcessGeneratedScheduleMessage(reqBody);

        return Ok();
    }

    //Endpoint to move the collection of player schedules into a bigger collection as an archieve
    [HttpPut("{AssignmentsId}/Archieve/PlayerSchedules")]
    public async Task<ActionResult> ArchievePlayerSchedules(string AssignmentsId) {
        try {
            var assignment = (LeaguePlayerSeasonAssignments) await _leagueService.GetData("leagueSeasonAssignments", AssignmentsId);
            if (assignment == null) {
                return NotFound();
            }

            var playerSchedules = assignment.PlayerFullSchedule;

            Dictionary<string, bool> upsertInfo = new Dictionary<string, bool>();
            upsertInfo["ArchievePlayerFullSchedule"] = true;

            Dictionary<string, object> updatedValues = new Dictionary<string, object>();
            updatedValues["ArchievePlayerFullSchedule"] = playerSchedules;

            await _leagueService.EditData("leagueSeasonAssignments", upsertInfo, updatedValues);

            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    //Endpoint to take in a generated schedule (aggregate) and format each possible game and sort them by date
    [HttpPut("{AssignmentsId}/FinalSeasonSchedule")]
    public async Task<ActionResult> ProcessFinalSeasonSchedule(string AssignmentsId) {
        try {
            var assignment = (LeaguePlayerSeasonAssignments) await _leagueService.GetData("leagueSeasonAssignments", AssignmentsId);
            if (assignment == null) {
                return NotFound();
            }

            var playerSchedules = assignment.PlayerFullSchedule;

            var allGames = assignment.FinalFullSchedule;

            Dictionary<string, bool> upsertInfo = new Dictionary<string, bool>();
            upsertInfo["FinalFullSchedule"] = true;

            foreach (var player in playerSchedules) {
                foreach (var game in player.Item2) {
                    Type currType = game.GetType();
                    if (!currType.IsClass  && !currType.IsValueType) {
                        continue;
                    }
                    allGames.Add((SingleGame) game);
                }
            }

            allGames = allGames.OrderBy(game => game.timePlayed).ToList();


            Dictionary<string, object> updatedValues = new Dictionary<string, object>();
            updatedValues["FinalFullSchedule"] = allGames;

            await _leagueService.EditData("leagueSeasonAssignments", upsertInfo, updatedValues);

            return Ok();
        }
        catch {
            return BadRequest();
        }
    }


    //Endpoint to move the collection of games into a bigger collection as an archieve
    [HttpPut("{AssignmentsId}/Archieve/FinalFullSchedules")]
    public async Task<ActionResult> ArchieveFullSchedules(string AssignmentsId) {
        try {
            var assignment = (LeaguePlayerSeasonAssignments) await _leagueService.GetData("leagueSeasonAssignments", AssignmentsId);
            if (assignment == null) {
                return NotFound();
            }

            var finalSchedules = assignment.FinalFullSchedule;

            Dictionary<string, bool> upsertInfo = new Dictionary<string, bool>();
            upsertInfo["ArchieveFinalFullSchedule"] = true;

            Dictionary<string, object> updatedValues = new  Dictionary<string, object>();
            updatedValues["ArchieveFinalFullSchedule"] = finalSchedules;

            await _leagueService.EditData("leagueSeasonAssignments", upsertInfo, updatedValues);

            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    [HttpGet("{AssignmentsId}/{playerName}/PlayerSchedule")]
    public async Task<ActionResult<List<SingleGame>>> GetPlayerSchedule(string AssignmentsId, string playerName) {
        var assignment = (LeaguePlayerSeasonAssignments) await _leagueService.GetData("leagueSeasonAssignments", AssignmentsId);
        if (assignment == null) {
            return NotFound();
        }

        var db = RedisConnector.db;
        if (db.KeyExists("assignment_" + playerName + "_schedule")) {
            var initSchedule = await db.StringGetAsync("assignment_" + playerName + "_schedule");
            var strSchedule = initSchedule.ToString();

            Dictionary<string, object> resBody = new Dictionary<string, object>();
            resBody["playerName"] = playerName;
            resBody["schedule"] = JsonConvert.DeserializeObject<List<SingleGame>>(strSchedule)!;

            OkObjectResult res = new OkObjectResult(resBody);

            return Ok(res);
        }

        var playerMatches = new List<SingleGame>();

        var playerSchedules = assignment.PlayerFullSchedule;
        foreach (var playerSchedule in playerSchedules) {
            if (playerSchedule.Item1 == playerName) {

                foreach (var game in playerSchedule.Item2) {
                    Type currType = game.GetType();
                    if (!currType.IsClass  && !currType.IsValueType) {
                        playerMatches.Add((SingleGame) playerSchedules[(int) ((List<Object>)game)[0]].Item2[(int)((List<Object>)game)[1]]);
                    }
                    else {
                        playerMatches.Add((SingleGame) game);
                    }
                }

                Dictionary<string, object> resBody = new Dictionary<string, object>();
                resBody["playerName"] = playerName;
                resBody["schedule"] = playerMatches;

                await db.StringSetAsync("assignment_" + playerName + "_schedule", JsonConvert.SerializeObject(playerMatches), TimeSpan.FromSeconds(3600));

                OkObjectResult res = new OkObjectResult(resBody);

                return Ok(res);
            }
        }

        return NotFound();
    }


    [HttpGet("{AssignmentsId}/FinalSchedule")]
    public async Task<ActionResult<List<SingleGame>>> GetFinalSchedule(string AssignmentsId) {
        var assignment = (LeaguePlayerSeasonAssignments) await _leagueService.GetData("leagueSeasonAssignments", AssignmentsId);
        if (assignment == null) {
            return NotFound();
        }

        OkObjectResult res = new OkObjectResult(assignment.FinalFullSchedule);
        return Ok(res);
    }
}