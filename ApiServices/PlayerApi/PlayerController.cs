namespace CompetitiveGamingApp.Controllers;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;
using KafkaHelper;
using Newtonsoft.Json;
using System.Text.Json;

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
public async Task<ActionResult<string>> CreatePlayer(Dictionary<string, object> infoContent)
{
    try
    {
        // Generate a new player object
        Player createdPlayer = new Player
        {
            playerId = Guid.NewGuid().ToString(),
            playerName = infoContent["name"].ToString(),
            playerUsername = infoContent["playerUsername"].ToString(),
            playerEmail = infoContent["playerEmail"].ToString(),
            playerJoined = DateTime.Now,
            playerAvailable = false,
            playerFriends = new List<string>(),
            leagueJoined = false,
            playerInGame = false,
            playerLeagueJoined = "",
            singleGamePrice = Convert.ToDouble(((JsonElement)infoContent["price"]).GetDouble()),
            enablePushNotifications = ((JsonElement)infoContent["pushNotifications"]).GetBoolean()
        };

        // Convert singlePlayerRecord list to byte array if provided
        if (infoContent.ContainsKey("singlePlayerRecord") && infoContent["singlePlayerRecord"] is JsonElement jsonElementRecord && jsonElementRecord.ValueKind == JsonValueKind.Array)
        {
            List<int> intList = new List<int>();
            foreach (var item in jsonElementRecord.EnumerateArray())
            {
                intList.Add(item.GetInt32());
            }
            createdPlayer.singlePlayerRecord = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(intList);
        }

        // Save the new player to the database
        await _playerService.AddAsync(createdPlayer);
        await _playerService.SaveChangesAsync();

        // Return the playerId of the created player
        return Ok(createdPlayer.playerId);
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
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
public async Task<ActionResult> JoinLeague(string username, string leagueName)
{
    try
    {
        // Retrieve the player using FirstOrDefaultAsync to fetch a single record
        var player = await _playerService.players
                                          .AsQueryable()
                                          .FirstOrDefaultAsync(user => user.playerUsername == username);
        
        if (player == null)
        {
            return NotFound($"Player with username {username} not found.");
        }

        // Update player's league information
        player.leagueJoined = true;
        player.playerLeagueJoined = leagueName;

        // Save changes asynchronously
        await _playerService.SaveChangesAsync();

        // Produce Kafka message
        //await _kafkaProducer.ProduceMessageAsync("JoinedLeagueStatus", leagueName, player.playerId!);

        return Ok();
    }
    catch (Exception ex)
    {
        return BadRequest($"An error occurred: {ex.Message}");
    }
}


  [HttpPut("{username}")]
public async Task<ActionResult> LeaveLeague(string username)
{
    try
    {
        // Retrieve the player using FirstOrDefaultAsync
        var player = await _playerService.players
                                          .AsQueryable()
                                          .FirstOrDefaultAsync(user => user.playerUsername == username);

        if (player == null)
        {
            return NotFound($"Player with username {username} not found.");
        }

        // Update player's league information
        player.leagueJoined = false;
        player.playerLeagueJoined = null;

        // Save changes asynchronously
        await _playerService.SaveChangesAsync();

        // Produce Kafka message
        //await _kafkaProducer.ProduceMessageAsync("LeftLeagueStatus", "left", player.playerId!);

        return Ok();
    }
    catch (Exception ex)
    {
        return BadRequest($"An error occurred: {ex.Message}");
    }
}


    [HttpPut("available/{username}/{status}")]
public async Task<ActionResult> ChangeAvailableStatus(string username, bool status)
{
    try
    {
        // Retrieve the player using FirstOrDefaultAsync
        var player = await _playerService.players
                                          .AsQueryable()
                                          .FirstOrDefaultAsync(user => user.playerUsername == username);

        if (player == null)
        {
            return NotFound($"Player with username {username} not found.");
        }

        // Update player's availability status
        player.playerAvailable = status;

        // Save changes asynchronously
        await _playerService.SaveChangesAsync();

        // Determine availability status as string
        string availableStatus = status ? "available" : "unavailable";

        // Produce Kafka messages
        //await _kafkaProducer.ProduceMessageAsync("ChangedAvailableStatus", availableStatus, player.playerId!);
        //await _kafkaProducer.ProduceMessageAsync("ChangedAvailableStatus", $"{player.playerName}_{availableStatus}", "app");

        return Ok();
    }
    catch (Exception ex)
    {
        return BadRequest($"An error occurred: {ex.Message}");
    }
}


  [HttpPut("playing/{username}/{status}")]
public async Task<ActionResult> ChangeGameStatus(string username, bool status)
{
    try
    {
        // Retrieve the player using FirstOrDefaultAsync to prevent loading unnecessary data
        var player = await _playerService.players
                                          .AsQueryable()
                                          .FirstOrDefaultAsync(user => user.playerUsername == username);
        
        if (player == null)
        {
            return NotFound($"Player with username {username} not found.");
        }

        // Update the player's in-game status
        player.playerInGame = status;
        
        // Save changes asynchronously
        await _playerService.SaveChangesAsync();

        string gameStatus = status ? "Playing in game" : "Open";

        // Produce Kafka messages
        //await _kafkaProducer.ProduceMessageAsync("ChangedGameStatus", gameStatus, player.playerId!);
        //await _kafkaProducer.ProduceMessageAsync("ChangedGameStatus", $"{player.playerName}_{gameStatus}", "app");

        return Ok();
    }
    catch (Exception ex)
    {
        return BadRequest($"An error occurred: {ex.Message}");
    }
}


   [HttpPut("{username}/{newPrice}/changeFee")]
public async Task<ActionResult> ChangeFee(string username, double newPrice)
{
    try
    {
        // Retrieve the player using FirstOrDefaultAsync
        var player = await _playerService.players
                                          .AsQueryable()
                                          .FirstOrDefaultAsync(user => user.playerUsername == username);
        
        if (player == null)
        {
            return NotFound($"Player with username {username} not found.");
        }

        // Update the single game price
        player.singleGamePrice = newPrice;
        
        // Save changes asynchronously
        await _playerService.SaveChangesAsync();

        // Produce Kafka messages
        //await _kafkaProducer.ProduceMessageAsync("ChangedSingleGameFee", $"{newPrice}", player.playerId!);
        //await _kafkaProducer.ProduceMessageAsync("ChangedSingleGameFee", $"{newPrice}", "app");

        return Ok();
    }
    catch (Exception ex)
    {
        return BadRequest($"An error occurred: {ex.Message}");
    }
}



[HttpPut("/record/{username}/{wins}/{losses}")]
public async Task<ActionResult> UpdatePlayerRecord(string username, int wins, int losses)
{
    try
    {
        // Fetch the player
        var player = await _playerService.players
                                         .AsQueryable()
                                         .Where(user => user.playerUsername == username)
                                         .FirstOrDefaultAsync();
                                         
        if (player == null)
        {
            return BadRequest("Player not found");
        }

        // Deserialize singlePlayerRecord from byte[] to List<int>
        List<int> record = new List<int>();
        if (player.singlePlayerRecord != null && player.singlePlayerRecord.Length > 0)
        {
            record = System.Text.Json.JsonSerializer.Deserialize<List<int>>(player.singlePlayerRecord);
        }

        // Ensure record has space for wins and losses (2 elements in this example)
        if (record.Count < 2)
        {
            record = new List<int> { 0, 0 }; // Initialize if empty or not enough elements
        }

        // Update wins and losses
        record[0] += wins;
        record[1] += losses;

        // Serialize back to byte[] and update player's singlePlayerRecord
        player.singlePlayerRecord = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(record);

        // Save changes
        await _playerService.SaveChangesAsync();

        // Produce Kafka message with updated record
        // await _kafkaProducer.ProduceMessageAsync("UpdatePlayerRecord", string.Join(",", record), player.playerId);

        return Ok();
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}


[HttpPut("/changedPushNotifications/{username}")]
public async Task<ActionResult> UpdatePushNotificationOption(string username)
{
    try
    {
        // Retrieve the player using FirstOrDefaultAsync to fetch a single record
        var player = await _playerService.players
                                          .AsQueryable()
                                          .FirstOrDefaultAsync(user => user.playerUsername == username);

        if (player == null)
        {
            return NotFound($"Player with username {username} not found.");
        }

        // Toggle push notification setting
        player.enablePushNotifications = !player.enablePushNotifications;

        // Save changes asynchronously
        await _playerService.SaveChangesAsync();

        return Ok(new { message = $"Push notification setting updated to {player.enablePushNotifications}" });
    }
    catch (Exception ex)
    {
        return BadRequest($"An error occurred: {ex.Message}");
    }
}

   
}