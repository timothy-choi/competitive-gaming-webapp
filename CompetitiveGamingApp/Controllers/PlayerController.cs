namespace CompetitiveGamingApp.Controllers;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/Players")]
public class PlayerController : ControllerBase {

    private readonly PlayerServices _playerService;
    public PlayerController(PlayerServices playerServices) {
        _playerService = playerServices;
    }

    [HttpGet]
    public async Task<ActionResult<List<Player>>> GetAllPlayers() {
        var players = await _playerService.players.AsQueryable().ToListAsync();
        OkObjectResult allPlayers = new OkObjectResult(players);
        return Ok(allPlayers);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<Player>> GetPlayer(string username) {
        try {
            var player = await _playerService.players.AsQueryable().Where(user => user.playerUsername == username).ToListAsync();
            if (player == null) {
                return BadRequest();
            }
            OkObjectResult resPlayer = new OkObjectResult(player);
            return Ok(resPlayer);
        }
        catch {
            return BadRequest();
        }
    }

}