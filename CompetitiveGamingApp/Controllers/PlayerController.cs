namespace CompetitiveGamingApp.Controllers;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;
using KafkaHelper;

[ApiController]
[Route("api/Players")]
public class PlayerController : ControllerBase {

    private readonly PlayerServices _playerService;
    private readonly KafkaProducer _kafkaProducer;
    public PlayerController(PlayerServices playerServices) {
        _playerService = playerServices;
        _kafkaProducer = new KafkaProducer();
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

    [HttpPost]
    public async Task<ActionResult<string>> CreatePlayer([FromBody] Dictionary<string, string> playerInfo) {
        try {
            Player createdPlayer = new Player {
                playerId = Guid.NewGuid().ToString(),
                playerName = playerInfo["name"],
                playerUsername = playerInfo["playerUsername"],
                playerEmail = playerInfo["playerEmail"],
                playerJoined = DateTime.Now,
                playerAvailable = false,
                playerFriends = new List<string>(),
                leagueJoined = false,
                playerInGame = false,
                playerLeagueJoined = "",
                singlePlayerRecord = new List<int>()
            };

            await _playerService.AddAsync(createdPlayer);
            await _playerService.SaveChangesAsync();

            OkObjectResult res = new OkObjectResult(createdPlayer.playerId);
            return Ok(res);
        }
        catch {
            return BadRequest();
        }
    }

    [HttpDelete("{username}")]
    public async Task<ActionResult> DeletePlayer(string username) {
        try {
            var player = await _playerService.players.AsQueryable().Where(user => user.playerUsername == username).ToListAsync();
            if (player == null) {
                return BadRequest();
            }
            var curr = player[0];
            _playerService.Remove(curr);
            await _playerService.SaveChangesAsync();
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("friends")]
    public async Task<ActionResult> AddFriend([FromBody] Dictionary<string, string> userInfo) {
        try {
            var player = await _playerService.players.AsQueryable().Where(user => user.playerUsername == userInfo["username"]).ToListAsync();
            if (player == null) {
                return BadRequest();
            }
            var singlePlayer = player[0];
            singlePlayer.playerFriends?.Add(userInfo["friendUsername"]);
            _playerService.SaveChanges();

            await _kafkaProducer.ProduceMessageAsync("addPlayerFriend", userInfo["friendUsername"], singlePlayer.playerId!);
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpDelete("{username}/{friendUsername}")]
    public async Task<ActionResult> RemoveFriend(string username, string friendUsername) {
        try {
            var player = await _playerService.players.AsQueryable().Where(user => user.playerUsername == username).ToListAsync();
            if (player == null) {
                return BadRequest();
            }
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

            await _kafkaProducer.ProduceMessageAsync("RemoveFromFriendsList", index.ToString(), singlePlayer.playerId!);
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("{username}/{leagueName}")]
    public async Task<ActionResult> JoinLeague(string username, string leagueName) {
        try {
            var player = await _playerService.players.AsQueryable().Where(user => user.playerUsername == username).ToListAsync();
            if (player == null) {
                return BadRequest();
            }
            player[0].leagueJoined = true;
            player[0].playerLeagueJoined = leagueName;
            _playerService.SaveChanges();

            await _kafkaProducer.ProduceMessageAsync("JoinedLeagueStatus", leagueName, player[0].playerId!);
            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

     [HttpPut("{username}")]
    public async Task<ActionResult> LeaveLeague(string username) {
        try {
            var player = await _playerService.players.AsQueryable().Where(user => user.playerUsername == username).ToListAsync();
            if (player == null) {
                return BadRequest();
            }
            player[0].leagueJoined = false;
            player[0].playerLeagueJoined = null;
            _playerService.SaveChanges();

            await _kafkaProducer.ProduceMessageAsync("LeftLeagueStatus", "left", player[0].playerId!);
            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    [HttpPut("available/{username}/{status}")]
    public async Task<ActionResult> changeAvailableStatus(string username, bool status) {
        try {
            var player = await _playerService.players.AsQueryable().Where(user => user.playerUsername == username).ToListAsync();
            if (player == null) {
                return BadRequest();
            }
            player[0].playerAvailable = status;
            _playerService.SaveChanges();

            string availableStatus = status ? "available" : "unavailable";

            await _kafkaProducer.ProduceMessageAsync("ChangedAvailableStatus", availableStatus, player[0].playerId!);

            //new call to kafka
            await _kafkaProducer.ProduceMessageAsync("ChangedAvailableStatus", player[0].playerName + "_" + availableStatus, "app");

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("playing/{username}/{status}")]
    public async Task<ActionResult> changeGameStatus(string username, bool status) {
        try {
            var player = await _playerService.players.AsQueryable().Where(user => user.playerUsername == username).ToListAsync();
            if (player == null) {
                return BadRequest();
            }
            player[0].playerInGame = status;
            _playerService.SaveChanges();

            string gameStatus = status ? "Playing in game" : "open";

            await _kafkaProducer.ProduceMessageAsync("ChangedGameStatus", gameStatus, player[0].playerId!);
            await _kafkaProducer.ProduceMessageAsync("ChangedGameStatus", player[0].playerName + "_" + gameStatus, "app");
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("/record/{username}/{wins}/{losses}")]
    public async Task<ActionResult> updatePlayerRecord(string username, int wins, int losses) {
        try {
            var player = await _playerService.players.AsQueryable().Where(user => user.playerUsername == username).ToListAsync();
            if (player == null) {
                return BadRequest();
            }
            player[0].singlePlayerRecord[0] += wins;
            player[0].singlePlayerRecord[1] += losses;
            _playerService.SaveChanges();

            var record = player[0].singlePlayerRecord;

            await _kafkaProducer.ProduceMessageAsync("UpdatePlayerRecord", string.Join(",", record!), player[0].playerId!);
            return Ok();
        } catch {
            return BadRequest();
        }
    }
}