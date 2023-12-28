namespace CompetitiveGamingApp.Controllers;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/playerPayment")]
public class PlayerPaymentController : ControllerBase {
    private readonly PlayerPaymentServices _playerPaymentService;
    public PlayerPaymentController(PlayerPaymentServices playerPaymentServices) {
        _playerPaymentService = playerPaymentServices;
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<PlayerPaymentAccount>> getPaymentAccount(string username) {
        try {
            var player = await _playerPaymentService.PlayerPaymentAccounts.AsQueryable().Where(user => user.playerUsername == username).ToListAsync();
            if (player == null) {
                return BadRequest();
            }
            OkObjectResult res = new OkObjectResult(player[0]);
            return Ok(res);
        } catch {
            return BadRequest();
        }
    }

    [HttpPost]
    public async Task<ActionResult> createNewAccount([FromBody] Dictionary<string, string> acctInfo) {
        try {
            PlayerPaymentAccount playerAcct = new PlayerPaymentAccount {
                playerPaymentAccountId = Guid.NewGuid().ToString(),
                playerUsername = acctInfo["username"],
                playerPaymentUsername = acctInfo["cashAppUsername"],
                playerCashAppId = acctInfo["playerCashAppId"],
                idempotencyKey = Guid.NewGuid().ToString()
            };

            await _playerPaymentService.AddAsync(playerAcct);
            await _playerPaymentService.SaveChangesAsync();
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpDelete("{username}")]
    public async Task<ActionResult> DeleteAccount(string username) {
        try {
            var acct = await _playerPaymentService.PlayerPaymentAccounts.AsQueryable().Where(user => user.playerUsername == username).ToListAsync();
            if (acct == null) {
                return NotFound();
            }
            
            _playerPaymentService.Remove(acct);
            await _playerPaymentService.SaveChangesAsync();
            return Ok();
        } catch {
            return BadRequest();
        }
    }
}