namespace CompetitiveGamingApp.Controllers;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;


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
public async Task<ActionResult> addNewGame(Dictionary<string, object> gameInfo) {
    try {
        SingleGamePaymentTransactions curr = new SingleGamePaymentTransactions {
            transactionId = Guid.NewGuid().ToString(),
            initPlayer = gameInfo["initPlayer"].ToString(),
            hostPlayer = gameInfo["hostPlayer"].ToString(),
            gameId = gameInfo["gameId"].ToString(),
            playerLost = gameInfo["playerLost"].ToString(),
            amountPaid = gameInfo["amountPaid"] is JsonElement amountElement && amountElement.ValueKind == JsonValueKind.String && amountElement.GetString() == ""
                          ? 0.00 
                          : Convert.ToDouble(((JsonElement)gameInfo["amountPaid"]).ToString()),
            timePaid = DateTime.Parse(((JsonElement)gameInfo["timePaid"]).GetString()),
            paymentId = gameInfo["paymentId"].ToString()
        };

        await _singleTransService.addNewGameResult(curr);
        return Ok();
    } catch (Exception e) {
        Console.WriteLine(e.Message);
        return BadRequest();
    }
}

}