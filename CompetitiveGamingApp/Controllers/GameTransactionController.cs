namespace CompetitiveGamingApp.Controllers;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/GameTransactions")]
public class GameTransactionController : ControllerBase {
    private readonly SingleGamePaymentTransactionsService _singleTransService;

    public GameTransactionController(SingleGamePaymentTransactionsService singleTransService) {
        _singleTransService = singleTransService;
    }

    [HttpGet]
    public async Task<ActionResult<List<SingleGamePaymentTransactions>?>> getAllGames() {
        try {
            var trans = await _singleTransService.getAllGames();
            OkObjectResult res = new OkObjectResult(trans);
            return Ok(res);
        } catch {
            return BadRequest();
        }
    }

    [HttpGet("{transId}")]
    public async Task<ActionResult<SingleGamePaymentTransactions>> getGame(string transId) {
        try {
            var trans = await _singleTransService.getPastGame(transId);
            if (trans == null) {
                return BadRequest();
            }
            OkObjectResult res = new OkObjectResult(trans);
            return Ok(res);
        } catch {
            return BadRequest();
        }
    }

    [HttpPost]
    public async Task<ActionResult> addNewGame([FromBody] Dictionary<string, string> gameInfo) {
        try {
            SingleGamePaymentTransactions curr = new SingleGamePaymentTransactions {
                transactionId = Guid.NewGuid().ToString(),
                initPlayer = gameInfo["initPlayer"],
                hostPlayer = gameInfo["hostPlayer"],
                gameId = gameInfo["gameId"],
                playerLost = gameInfo["playerLost"],
                amountPaid = gameInfo["amountPaid"] == "" ? 0.00 : Convert.ToDouble(gameInfo["amountPaid"]),
                timePaid = Convert.ToDateTime(gameInfo["timePaid"])
            };
            await _singleTransService.addNewGameResult(curr);
            return Ok();
        } catch {
            return BadRequest();
        }
    }
}