namespace CompetitiveGamingApp.Controller;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;
using System;

[ApiController]
[Route("api/LeagueSeasonAssignments")]
public class LeagueSeasonAssignmentsController : ControllerBase {
    private readonly MongoDBService _leagueService;
    public LeagueSeasonAssignmentsController(MongoDBService leagueService) {
        _leagueService = leagueService;
    }

    [HttpGet("{AssignmentId}")]
    public async Task<ActionResult<LeaguePlayerSeasonAssignments>> GetSeasonAssignments(string AssignmentId) {
        var assignment = _leagueService.GetData("leagueSeasonAssignments", AssignmentId);
        if (assignment.Count == 0) {
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
                ConfigId = reqBody["ConfigId"],
                LeagueId = reqBody["LeagueId"]
                PartitionsEnabled = reqBody["PartitionsEnabled"],
                ReassignEverySeason = reqBody["ReassignEverySeason"],
                AutomaticInduction = reqBody["AutomaticInduction"],
                NumberOfPlayersPerPartition = reqBody["NumberOfPlayersPerPartition"],
                NumberOfPartitions = reqBody["NumberOfPartitions"],
                SamePartitionSize = reqBody["SamePartitionSize"],
                AutomaticScheduling = reqBody["AutomaticScheduling"],
                ExcludeOutsideGames = reqBody["ExcludeOutsideGames"],
                InterDivisionGameLimit = reqBody["InterDivisionGameLimit"],
                RepeatMatchups = reqBody["RepeatMatchups"],
                MaxRepeatMatchups = reqBody["MaxRepeatMatchups"],
                DivisionSelective = reqBody["DivisionSelective"],
                OutsideDivisionSelections = new Dictionary<string, List<string>>(),
                RandomizeDivisionSelections = reqBody["RandomizeDivisionSelections"],
                PlayerSelection = reqBody["PlayerSelection"],
                PlayerExemptLists = new Dictionary<string, List<string>>(),
                repeatAllMatchups = reqBody["repeatAllMatchups"],
                minRepeatMatchups = reqBody["minRepeatMatchups"],
                maxRepeatMatchups = reqBody["maxRepeatMatchups"],
                playAllPlayers = reqBody["playAllPlayers"],
                AllPartitions = new Dictionary<String, List<String>>(),
                AllCombinedDivisions = new Dictionary<String, List<String>>(),
                PlayerFullSchedule = new List<Tuple<string, List<SingleGame>>>(),
                ArchievePlayerFullSchedule = new List<List<Tuple<string, List<SingleGame>>>>(),
                FinalFullSchedule = new List<SingleGame>(),
                ArchieveFinalFullSchedule = new List<List<SingleGame>>()
            };

            await _leagueService.PostData("leagueSeasonAssignments", currLeague);

            OkObjectResult res = new OkObjectResult(currLeague.AssignmentsId);
            return Ok(res);
        } catch {
            return BadRequest()       
        }
    }

    [HttpPut("{AssignmentsId}")]
    public async Task<ActionResult> EditSeasonAssignmentsOptions(string AssignmentsId, Dictionary<string, object> reqBody) {
        try {
            var assignment = _leagueService.GetData("leagueSeasonAssignments", AssignmentId);
            if (assignment.Count == 0) {
                return NotFound();
            }

            Dictionary<string, bool> upsertInfo;
            Dictionary<string, object> updatedValues;

            for (var setting in reqBody) {
                upsertInfo[setting] = reqBody[setting].Item1;
                updatedValues[setting] = reqBody[setting].Item2;
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
            var assignment = _leagueService.GetData("leagueSeasonAssignments", AssignmentId);
            if (assignment.Count == 0) {
                return NotFound();
            }

            Dictionary<string, bool> upsertInfo;
            Dictionary<string, object> updatedValues;

            upsertInfo[reqBody["indexName"]] = reqBody["upsertStatus"];

            if (reqBody["deleteOption"]) {
                var index = -1;
                var selections = assignment.OutsideDivisionSelections;
                for (int i = 0; i < selections.Count; ++i) {
                    if (reqBody["selectedIndex"] == i) {
                        index = i;
                        break;
                    }
                }

                if (index == -1) {
                    return NotFound();
                }

                selections.removeAt(index);
                updatedValues[reqBody["indexName"]] = selections;
            }
            else {
                updatedValues[reqBody["indexName"]] = reqBody["DivisionValues"];
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
            var assignment = _leagueService.GetData("leagueSeasonAssignments", AssignmentId);
            if (assignment.Count == 0) {
                return NotFound();
            }
            
            var divisionSelection = assignment.OutsideDivisionSelections;

            Dictionary<string, bool> upsertInfo;
            upsertInfo["OutsideDivisionSelections"] = false;
            Dictionary<string, object> updatedValues;

            Dictionary<string, string> player_divisions;
            for (int x = 0; x < reqBody["players"].Count; ++x) {
                for (int y = 0; y < reqBody["divisions"].Count; ++i) {
                    var found = reqBody["division_assignment"][reqBody["divisions"][y]].Find(player => player == reqBody["players"][x]);
                    if (found != -1) {
                        player_divisions[reqBody["players"][x]] = reqBody["divisions"][y];
                        break;
                    }
                }
            }

            for (int i = 0; i < reqBody["players"].Count; ++i) {
                var random = new Random();
                var divisions = reqBody["divisions"];
                while (divisionSelection[reqBody["players"][i]].Count != reqBody["num_of_selections"]) {
                    var selection = divisions[random.Next(0, divisions.Count-1)];
                    divisionSelection[reqBody["players"][i]].add(selection);
                    divisions.removeAll(d => d == selection);
                    for (int j = 0; j < reqBody["division_assignment"][selection].Count; ++j) {
                        divisionSelection[reqBody["division_assignment"][selection][j]].add(player_divisions[reqBody["players"][i]]);
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
            var assignment = _leagueService.GetData("leagueSeasonAssignments", AssignmentId);
            if (assignment.Count == 0) {
                return NotFound();
            }

            Dictionary<string, bool> upsertInfo;
            upsertInfo[reqBody["player_list_key"]] = false;
            Dictionary<string, object> updatedValues;

            updatedValues[reqBody["player_list_key"]] = reqBody["exempt_lists"];

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
            var assignment = _leagueService.GetData("leagueSeasonAssignments", AssignmentId);
            if (assignment.Count == 0) {
                return NotFound();
            }

            Dictionary<string, bool> upsertInfo;

            Dictionary<string, object> updatedValues;

            for (var player in reqBody) {
                for (var update in reqBody[player]) {
                    upsertInfo[update.Item2] = update.Item1 ? false : true;
                    if (!update.Item1) {
                        var exemptList = assignment.ExemptLists[player];

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
            var assignment = _leagueService.GetData("leagueSeasonAssignments", AssignmentId);
            if (assignment.Count == 0) {
                return NotFound();
            }

            var count = 0;

            Dictionary<string, bool> upsertInfo;
            upsertInfo["AllPartitions"] = false;

            Dictionary<string, object> updatedValues;

            var partitions = assignment.AllPartitions;

            var seen = new List<string>();
            for (var division in reqBody) {
                partitions[division] = reqBody[division]
                for (var player in reqBody[division]) {
                    seen.add(player);
                    count++;
                }
            }

            if (seen.Distinct.Count() != count) {
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
            var assignment = _leagueService.GetData("leagueSeasonAssignments", AssignmentId);
            if (assignment.Count == 0) {
                return NotFound();
            }

            Dictionary<string, bool> upsertInfo;
            upsertInfo["AllPartitions"] = false;

            Dictionary<string, object> updatedValues;

            var partitions = assignment.AllPartitions;

            var players = reqBody["players"];

            var min_num_players_on_group = reqBody["num_num_players_per_group"];

            var random = new Random();

            for (var division in reqBody["divisions"]) {
                for (int i = 0; i < min_num_players_on_group; ++i) {
                    var selection = players[random.Next(0, players.Count-1)];
                    players.removeAll(player => player == selection);
                    partitions[division].add(selection);
                }
            }

            if (players.Count > 0) {
                random = new Random();
                var divisions = reqBody["divisions"];
                while (players.Count > 0) {
                    var div_selection = divisions[random.Next(0, divisions.Count-1)];
                    var player_selection = players[random.Next(0, players.Count-1)];
                    players.removeAll(player => player == player_selection);

                    partitions[div_selection].add(player_selection);
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
    public async Task<ActionResults> EditDivisions(string AssignmentsId, Dictionary<string, object> reqBody) {
        try {
            var assignment = _leagueService.GetData("leagueSeasonAssignments", AssignmentId);
            if (assignment.Count == 0) {
                return NotFound();
            }

            Dictionary<string, bool> upsertInfo;
            upsertInfo["AllPartitions"] = reqBody["remove"];

            Dictionary<string, object> updatedValues;

            if (!upsertInfo["AllPartitions"]) {
                var divisionAssignment = assignment.AllPartitions[reqBody["target_division"]];
                divisionAssignment.removeAll(player => player == reqBody["selected_player"]);
                updatedValues["AllPartitions"] = divisionAssignment;
            }
            else {
                updatedValues[reqBody["division_key"]] = reqBody["new_player"];  
            }

            await _leagueService.EditData("leagueSeasonAssignments", upsertInfo, updatedValues);

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpGet("{AssignmentsId}/Divisions")]
    public async Task<ActionResult<Dictionary<string, List<string>>>> GetCurrentDivisionSelections(string AssignmentsId) {
        var assignment = _leagueService.GetData("leagueSeasonAssignments", AssignmentsId);
        if (assignment.Count == 0) {
            return NotFound();
        }

        OkObjectResult res = new OkObjectResult(assignment.AllPartitions);

        return Ok(res);
    }

    [HttpPost("{AssignmentsId}/Combined")]
    public async Task<ActionResult> AddCombinedDivisionSelections(string AssignmentsId, Dictionary<string, object> reqBody) {
        try {
            var assignment = _leagueService.GetData("leagueSeasonAssignments", AssignmentsId);
            if (assignment.Count == 0) {
                return NotFound();
            }

            Dictionary<string, bool> upsertInfo;
            upsertInfo["AllCombinedDivisions"] = false;

            Dictionary<string, object> updatedValues;

            var combinedDivisions = assignment.AllCombinedDivisions;

            for (var combDivision in reqBody) {
                combinedDivisions[combDivision] = reqBody[combDivision];
            }

            updatedValues["AllCombinedDivisions"] = combinedDivisions;

            await _leagueService.EditData("leagueSeasonAssignments", upsertInfo, updatedValues);

            return Ok();
        } catch {
            return BadRequest();
        }
    }
}