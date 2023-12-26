namespace CompetitiveGamingApp.Controllers;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using System.Numerics;
using Newtonsoft.Json;

[ApiController]
[Route("api/singleGame")]

public class SingleGameController : ControllerBase {

    private readonly HttpClient client;
    private readonly SingleGameServices _singleGameService;
    public SingleGameController(SingleGameServices singleGameServices) {
        _singleGameService = singleGameServices;
        client = new HttpClient();
    }

    [HttpGet]
    public async Task<ActionResult<List<SingleGame>>> getAllGames() {
        List<SingleGame>? allGames = await _singleGameService.GetAllGames();
        OkObjectResult res = new OkObjectResult(allGames);
        return Ok(res);
    }

    [HttpGet("{gameId}")]
    public async Task<ActionResult<SingleGame>> getGame(string gameId) {
        try {
            SingleGame? game = await _singleGameService.GetGame(gameId);
            if (game == null) {
                return NotFound();
            }
            OkObjectResult res = new OkObjectResult(game);
            return Ok(res);
        } catch {
            return BadRequest();
        }
    }

    [HttpPost]
    public async Task<ActionResult> createNewGame([FromBody] Dictionary<string, string> gameInfo) {
        try {
            SingleGame scheduledGame = new SingleGame {
                SingleGameId = Guid.NewGuid().ToString(),
                hostPlayer = gameInfo["hostPlayer"],
                guestPlayer = gameInfo["guestPlayer"],
                finalScore = null,
                inGameScores = new List<Tuple<string, Tuple<int, int>>>(),
                timePlayed = DateTime.Parse(gameInfo["gametime"]),
                videoObjName = gameInfo["hostPlayer"] + "_videos",
                gameEditor = null,
                twitchBroadcasterId = null
            };

            await _singleGameService.CreateGame(scheduledGame);

            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    [HttpPost("/finalScore")]
    public async Task<ActionResult> addFinalScore([FromBody] Dictionary<string, string> finalScoreInfo) {
        try {
            Tuple<int, int> finalScore = Tuple.Create(Convert.ToInt32(finalScoreInfo["guestPoints"]), Convert.ToInt32(finalScoreInfo["hostPoints"]));

            await _singleGameService.UpdateFinalScore(finalScore, finalScoreInfo["gameId"]);

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("/editor/{gameId}/{editor}")]
    public async Task<ActionResult> updateGameEditor(string gameId, string editor) {
        try {
            var player = await client.GetAsync("/player/" + editor);
            if (player == null) {
                return NotFound();
            }
            await _singleGameService.EditUserGameEditor(editor, gameId);
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("/inGameScores")]
    public async Task<ActionResult> AddInGameScore([FromBody] Dictionary<string, string> inGameScoreInfo) {
        try {
            Tuple<int, int> score = Tuple.Create(Convert.ToInt32(inGameScoreInfo["guestScore"]), Convert.ToInt32(inGameScoreInfo["hostScore"]));
            Tuple<String, Tuple<int, int>> gameScore = Tuple.Create(inGameScoreInfo["gameScoreType"], score);

            await _singleGameService.AddInGameScores(gameScore, inGameScoreInfo["gameId"]);
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("/otherGameInfo")]
    public async Task<ActionResult> AddOtherGameInfo([FromBody] Dictionary<string, string> otherGameInfo) {
        try {
            Dictionary<string, string> parsedGameInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(otherGameInfo["gameInfo"])!; 
            await _singleGameService.AddOtherGameInfo(parsedGameInfo!, otherGameInfo["gameId"]);
            return Ok();
        } catch {
            return BadRequest();
        }
    }
}
