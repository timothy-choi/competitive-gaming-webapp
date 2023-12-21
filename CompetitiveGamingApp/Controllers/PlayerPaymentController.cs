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
}