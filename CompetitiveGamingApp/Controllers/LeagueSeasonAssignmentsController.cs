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
}