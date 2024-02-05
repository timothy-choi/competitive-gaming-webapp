namespace CompetitiveGamingApp.Controller;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;

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
}