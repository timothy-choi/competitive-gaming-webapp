namespace CompetitiveGamingApp.Controllers;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

[ApiController]
[Route("api/singleGame")]

public class SingleGameController : ControllerBase {

    private readonly SingleGameServices _singleGameService;
    public SingleGameController(SingleGameServices singleGameServices) {
        _singleGameService = singleGameServices;
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
}
