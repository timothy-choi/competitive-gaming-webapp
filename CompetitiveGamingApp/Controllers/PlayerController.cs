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
            OkObjectResult resPlayer = new OkObjectResult(player[0]);
            return Ok(resPlayer);
        }
        catch {
            return BadRequest();
        }
    }

    [HttpGet("friends/{username}")]
    public async Task<ActionResult<List<String>>> GetPlayerFriends(string username) {
        try {
            var player = await _playerService.players.AsQueryable().Where(user => user.playerUsername == username).ToListAsync();
            if (player == null) {
                return BadRequest();
            }
            OkObjectResult friends = new OkObjectResult(player[0].playerFriends);
            return Ok(friends);
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("friends")]
    public async Task<ActionResult> AddFriend([FromBody] Dictionary<string, string> userInfo) {
        try {
            var player = await _playerService.players.AsQueryable().Where(user => user.playerUsername == userInfo["username"]).ToListAsync();
            var singlePlayer = player[0];
            singlePlayer.playerFriends?.Add(userInfo["friendUsername"]);
            _playerService.SaveChanges();
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpDelete("{username}/{friendUsername}")]
    public async Task<ActionResult> RemoveFriend(string username, string friendUsername) {
        try {
            var player = await _playerService.players.AsQueryable().Where(user => user.playerUsername == username).ToListAsync();
            var singlePlayer = player[0];
            var index = -1;
            for (int i = 0; i < singlePlayer.playerFriends?.Count; ++i) {
                if (singlePlayer.playerFriends[i].Equals(friendUsername)) {
                    index = i;
                    break;
                }
            }
            if (index == -1) {
                return BadRequest();
            }
            singlePlayer.playerFriends?.RemoveAt(index);
            _playerService.SaveChanges();
            return Ok();
        } catch {
            return BadRequest();
        }
    }
}