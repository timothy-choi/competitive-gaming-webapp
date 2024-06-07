namespace CompetitiveGamingApp.Controllers;

using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Services;
using CompetitiveGamingApp.Models;
using Microsoft.EntityFrameworkCore;

using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Linq;
using RabbitMQ;
using System.Reflection.PortableExecutable;

[ApiController]
[Route("api/Recommendation")]


public class RecommendationController : ControllerBase {
    private PlayerRecommendationServices _playerServices;

    private LeagueRecommendationServices _leagueServices;
    private Producer _producer;
    private Consumer _consumer;
    public RecommendationController(PlayerRecommendationServices playerServices, LeagueRecommendationServices leagueServices) {
        _playerServices = playerServices;
        _leagueServices = leagueServices;
        _producer = new Producer();
        _consumer = new Consumer();
    }

    [HttpGet("/Player/${PlayerRecommendationId}")]
    public async Task<ActionResult<PlayerRecommendations>> GetPlayerRecommendations(string PlayerRecommedationId) {
        try {
            var player = await _playerServices.RecommendationAccounts.AsQueryable().Where(user => user.PlayerRecommendationId == PlayerRecommedationId).ToListAsync();
            if (player == null) {
                return BadRequest();
            }
            OkObjectResult res = new OkObjectResult(player[0]);
            return Ok(res);
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("/CreateRecommendationsProfile")]
    public async Task<ActionResult> CreateRecommendationsProfile([FromBody] Dictionary<string, string> reqBody) {
        try {
            PlayerRecommendations player = new PlayerRecommendations {
                PlayerRecommendationId = Guid.NewGuid().ToString(),
                PlayerId = reqBody["PlayerId"],
                PlayerUsername = reqBody["PlayerUsername"],
                PlayerHistoryRecords = new List<PlayerGameRecord?>()
            };

            await _playerServices.AddAsync(player);
            await _playerServices.SaveChangesAsync();

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("{playerUsername}/PlayerRecord")]
    public async Task<ActionResult> AddNewPlayerRecord(string playerUsername, [FromBody] Dictionary<string, string> reqBody) {
        try {
            PlayerGameRecord currRec = new PlayerGameRecord {
                PlayerGameRecordId = Guid.NewGuid().ToString(),
                PlayerId = reqBody["PlayerId"],
                PlayerUsername = reqBody["PlayerUsername"],
                PlayerRecord = reqBody["PlayerRecord"].Split(',').Select(int.Parse).ToList(),
                PlayerLeagueJoined = Convert.ToBoolean(reqBody["PlayerLeagueJoined"]),
                PlayerLeague = reqBody["PlayerLeague"],
                PlayerLeagueTags = reqBody["PlayerRecord"].Split(',').ToList()
            };

            var player = await _playerServices.RecommendationAccounts.AsQueryable().Where(user => user.PlayerRecommendationId == reqBody["PlayerRecommedationId"]).ToListAsync();
            if (player == null) {
                return BadRequest();
            }

            player[0].PlayerHistoryRecords.Add(currRec);
            await _playerServices.SaveChangesAsync();

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpGet("{playerUsername}/FindRecommendations")]
    public async Task<ActionResult<List<String>>> findUserRecommendations(string playerUsername) {
        try {
            Dictionary<string, object> msg = new Dictionary<string, object>();

            msg["val"] = playerUsername;

            _producer.SendMessage("player_rec_notifications", msg);

            List<String> recommendations = await _consumer.ReceiveUserRecommendations(playerUsername);

            OkObjectResult res = new OkObjectResult(recommendations);

            return Ok(res);
        } catch {
            return BadRequest();
        }
    }

    [HttpGet("/Player/${PlayerRecommendationId}")]
    public async Task<ActionResult<LeagueRecommendations>> GetLeagueRecommendations(string LeagueRecommedationId) {
        try {
            var player = await _leagueServices.RecommendationAccounts.AsQueryable().Where(user => user.LeagueRecommendationId == LeagueRecommedationId).ToListAsync();
            if (player == null) {
                return BadRequest();
            }
            OkObjectResult res = new OkObjectResult(player[0]);
            return Ok(res);
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("/CreateRecommendationsProfile")]
    public async Task<ActionResult> CreateLeagueRecommendationsProfile([FromBody] Dictionary<string, string> reqBody) {
        try {
            LeagueRecommendations player = new LeagueRecommendations {
                LeagueRecommendationId = Guid.NewGuid().ToString(),
                PlayerId = reqBody["PlayerId"],
                PlayerUsername = reqBody["PlayerUsername"],
                LeagueJoinedHistoryRecord = new List<LeagueJoinRecord?>()
            };

            await _leagueServices.AddAsync(player);
            await _leagueServices.SaveChangesAsync();

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("{LeagueName}/LeagueJoinRecord")]
    public async Task<ActionResult> AddNewLeagueJoinRecord(string LeagueName, [FromBody] Dictionary<string, string> reqBody) {
        try {
            LeagueJoinRecord league = new LeagueJoinRecord {
                LeagueJoinRecordId = Guid.NewGuid().ToString(),
                LeagueId = reqBody["LeagueId"],
                LeagueName = reqBody["LeagueName"],
                LeagueTags = reqBody["PlayerRecord"].Split(',').ToList(),
                LeaguePlayerOverallRecord = reqBody["LeaguePlayerOverallRecord"].Split(',').Select(int.Parse).ToList(),
                LeagueIndividualOverallRecord = reqBody["LeagueIndividualOverallRecord"].Split(';').Select(sub => sub.Split(',').Select(int.Parse).ToList()).ToList()
            };

            var player = await _leagueServices.RecommendationAccounts.AsQueryable().Where(user => user.LeagueRecommendationId == reqBody["LeagueRecommedationId"]).ToListAsync();
            if (player == null) {
                return BadRequest();
            }

            player[0].LeagueJoinedHistoryRecord.Add(league);

            await _leagueServices.SaveChangesAsync();

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    


}


