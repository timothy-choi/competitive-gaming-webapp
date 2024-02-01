namespace CompetitiveGamingApp.Controller;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;

[ApiController]
[Route("api/League")]
public class LeagueController : ControllerBase {
    private readonly MongoDBService _leagueService;
    public LeagueController(MongoDBService leagueService) {
        _leagueService = leagueService;
    }

    [HttpGet]
    public async Task<ActionResult<List<League>>> GetAllLeagues() {
       var allLeagues = await _leagueService.GetAllData("leagueInfo");
       OkObjectResult res = new OkObjectResult(allLeagues);
       return Ok(allLeagues); 
    }

    [HttpGet("{LeagueId}")]
    public async Task<ActionResult<League>> GetLeagueById(string LeagueId) {
        var league = await _leagueService.GetData("leagueInfo", LeagueId);
        OkObjectResult res = new OkObjectResult(league);
        return Ok(res);
    }

    [HttpPost]
    public async Task<ActionResult> CreateLeague(Dictionary<string, object> leagueInput) {
        try {
            League curr = new League {
                LeagueId = Guid.NewGuid().ToString(),
                Name = leagueInput["LeagueName"],
                Owner = leagueInput["LeagueOwner"],
                Description = leagueInput["LeagueDescription"],
                Players = new List<Dictionary<String, Object?>>(),
                tags = new List<string>(),
                LeagueConfig = "",
                SeasonAssignments = "",
                LeagueStandings = new List<Tuple<String, Dictionary<String, Dictionary<String, object>>>>(),
                AchieveLeagueStandings = new List<List<Tuple<String, Dictionary<String, Dictionary<String, object>>>>>(),
                DivisionStandings = new Dictionary<String, List<Tuple<String, Dictionary<String, object>>>>(),
                AchieveDivisionStandings = new List<Dictionary<String, List<Tuple<String, Dictionary<String, object>>>>>(),
                CombinedDivisionStandings = new Dictionary<String, List<Tuple<String, Dictionary<String, object>>>>(),
                AchieveCombinedDivisionStandings = new List<Dictionary<String, List<Tuple<String, Dictionary<String, object>>>>>(),
                Champions = new List<Tuple<String, String>>(),
                PlayoffAssignments = ""
            };

            await _leagueService.PostData("leagueInfo", curr);
            OkObjectResult res = new OkObjectResult(curr.LeagueId);
            return Ok(res);
        }
        catch {
            return BadRequest();
        }
    }

    [HttpDelete("{LeagueId}")]
    public async Task<ActionResult> DeleteLeague(string LeagueId) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }
            await _leagueService.DeleteData("leagueInfo", LeagueId);
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("{LeagueId}/{SeasonAssignmentsId}")]
    public async Task<ActionResult> SetSeasonConfig(string LeagueId, string SeasonAssignmentsId) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }
            Dictionary<String, String> body;
            body["AssignmentsId"] = SeasonAssignmentsId;

            Dictionary<string, bool> upsertStatus;
            upsertStatus["AssignmentsId"] = false;
            await _leagueService.EditData("leagueInfo", upsertStatus, body);
            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    [HttpPut("{LeagueId}/{LeagueConfigId}")]
    public async Task<ActionResult> SetConfigId(string LeagueId, string LeagueConfigId) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }
            Dictionary<String, String> body;
            body["ConfigId"] = LeagueConfigId;

            Dictionary<string, bool> upsertStatus;
            upsertStatus["ConfigId"] = false;
            await _leagueService.EditData("leagueInfo", upsertStatus, body);
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("{LeagueId}/{PlayoffAssigmentId}")]
    public async Task<ActionResult> SetPlayoffAssignments(string LeagueId, string PlayoffAssignmentId) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }
            Dictionary<String, String> body;
            body["PlayoffAssignmentId"] = PlayoffAssignmentId;

            Dictionary<string, bool> upsertStatus;
            upsertStatus["PlayoffAssignmentId"] = false;
            await _leagueService.EditData("leagueInfo", upsertStatus, body);
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("{LeagueId}/tags/{TagValue}")] 
    public async Task<ActionResult> AddNewTag(string LeagueId, string TagValue) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }
            Dictionary<String, String> body;
            body["tag"] = TagValue;

            Dictionary<string, bool> upsertStatus;
            upsertStatus["tag"] = true;
            await _leagueService.EditData("leagueInfo", upsertStatus, body);
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("{LeagueId}/players/{PlayerId}")]
    public async Task<ActionResult> AddNewPlayer(string LeagueId, string PlayerId) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }
            Dictionary<string, bool> upsertStatus;
            upsertStatus["Players"] = true;

            Dictionary<string, string> playerInfo;
            playerInfo["PlayerId"] = PlayerId;
            playerInfo["DateJoined"] = DateTime.Now;
            Dictionary<String, object> body;
            body["Players"] = playerInfo;

            await _leagueService.EditData("leagueInfo", upsertStatus, body);
            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

}


