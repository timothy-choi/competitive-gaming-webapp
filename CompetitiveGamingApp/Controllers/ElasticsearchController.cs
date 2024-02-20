namespace CompetitiveGamingApp.Controllers;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;
using Elastic.Clients.Elasticsearch;
using CompetitiveGamingApp;
using Elastic.Transport;
using Newtonsoft.Json;

[ApiController]
[Route("api/Search")]

public class ElasticsearchController : ControllerBase {
    private readonly ElasticsearchClient _client;
    public ElasticsearchController() {
        _client = new ElasticsearchClient("", new ApiKey(Environment.GetEnvironmentVariable("Elasticsearch_API_Key")!));
        try {
            _client.Index("Player");
        } catch {}

        try {
            _client.Index("Game");
        } catch {}

        try {
            _client.Index("League");
        } catch {}
    }

    [HttpPost("Player")]
    public async Task<ActionResult> AddPlayer(Dictionary<string, object> reqBody) {
        try {
            var player = new PlayerInfo {
                Id = Guid.NewGuid().ToString(),
                Name = reqBody["Name"].ToString(),
                Username = reqBody["Username"].ToString(),
                PlayerId = reqBody["PlayerId"].ToString()
            };

            await _client.IndexAsync(player, "Player");

            return Ok();

        } catch {
            return BadRequest();
        }
    }

    [HttpDelete("Player/{PlayerInfoId}")]
    public async Task<ActionResult> RemovePlayer(string PlayerInfoId) {
        try {
            await _client.DeleteAsync("Player", PlayerInfoId);
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpGet("Player")]
    public async Task<ActionResult<List<PlayerInfo>>> GetAllPlayers() {
        try {
            var db = RedisConnector.db;
            if (db.KeyExists("all_players")) {
                var results = await db.StringGetAsync("all_players");
                string strVersion = results.ToString();
                List<PlayerInfo> allPlayerList = JsonConvert.DeserializeObject<List<PlayerInfo>>(strVersion)!;
                OkObjectResult res = new OkObjectResult(allPlayerList);
                return Ok(res);
            }
            var searchResponse = await _client.SearchAsync<PlayerInfo>(s => s
                .Index("Player") 
                .Query(q => q
                    .MatchAll() 
                )
            );

            if (searchResponse.IsValidResponse) {
                OkObjectResult res = new OkObjectResult(searchResponse.Documents);

                var strList = JsonConvert.SerializeObject(searchResponse.Documents);

                await db.StringSetAsync("all_players", strList, TimeSpan.FromSeconds(3600));

                return Ok(res);
            }
            else {
                return BadRequest();
            }
        } catch {
            return BadRequest();
        }
    }

    [HttpGet("Player/{query}")]
    public async Task<ActionResult<List<PlayerInfo>>> SearchPlayerResults(string query) {
        try {
            var db = RedisConnector.db;
            if (db.KeyExists(query)) {
                var results = await db.StringGetAsync(query);
                string strVersion = results.ToString();
                List<PlayerInfo> allPlayerList = JsonConvert.DeserializeObject<List<PlayerInfo>>(strVersion)!;
                OkObjectResult res2 = new OkObjectResult(allPlayerList);
                return Ok(res2);
            }
            var allPlayers = new List<PlayerInfo>();
            var searchResponse = await _client.SearchAsync<PlayerInfo>(s => s
                .Index("Player") // Specify the index name
                .Query(q => q
                    .Fuzzy(m => m
                        .Field(f => f.Name) // Specify the field to search
                        .Value(query) // Specify the search query
                        .Fuzziness(new Fuzziness(1))
                    )
                )
            );
            if (searchResponse.IsValidResponse) {
                allPlayers.AddRange((List<PlayerInfo>)searchResponse.Hits);
            }
            else {
                return BadRequest();
            }

            searchResponse = await _client.SearchAsync<PlayerInfo>(s => s
                .Index("Player") // Specify the index name
                .Query(q => q
                    .Fuzzy(m => m
                        .Field(f => f.Username) // Specify the field to search
                        .Value(query) // Specify the search query
                        .Fuzziness(new Fuzziness(1))
                    )
                )
            );

            if (searchResponse.IsValidResponse)
            {
                allPlayers.AddRange((List<PlayerInfo>)searchResponse.Hits);
            }
            else
            {
                return BadRequest();
            }

            var strList = JsonConvert.SerializeObject(allPlayers);
            
            await db.StringSetAsync(query, strList, TimeSpan.FromSeconds(3600));
            
            OkObjectResult res = new OkObjectResult(allPlayers);

            return Ok(res);
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("Game/{query}")]
    public async Task<ActionResult> AddGame(Dictionary<string, object> reqBody) {
        try {
            var game = new GameInfo {
                Id = Guid.NewGuid().ToString(),
                Matchup = reqBody["matchup"].ToString(),
                GameId = reqBody["GameId"].ToString()
            };

            await _client.IndexAsync(game, "Game");

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpDelete("Game/{GameInfoId}")]
    public async Task<ActionResult> RemoveGame(string GameInfoId) {
        try {
            await _client.DeleteAsync("Game", GameInfoId);
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpGet("Game")]
    public async Task<ActionResult<List<GameInfo>>> GetAllGames() {
        try {
            var db = RedisConnector.db;
            if (db.KeyExists("all_games")) {
                var results = await db.StringGetAsync("all_games");
                string strVersion = results.ToString();
                List<GameInfo> allGameList = JsonConvert.DeserializeObject<List<GameInfo>>(strVersion)!;
                OkObjectResult res2 = new OkObjectResult(allGameList);
                return Ok(res2);
            }
            var searchResponse = await _client.SearchAsync<GameInfo>(s => s
                .Index("Game") 
                .Query(q => q
                    .MatchAll() 
                )
            );

            if (searchResponse.IsValidResponse) {
                var strList = JsonConvert.SerializeObject(searchResponse.Documents);
                await db.StringSetAsync("all_games", strList, TimeSpan.FromSeconds(3600));
                OkObjectResult res = new OkObjectResult(searchResponse.Documents);

                return Ok(res);
            }
            else {
                return BadRequest();
            }
        } catch {
            return BadRequest();
        }
    }


    [HttpGet("Game/{query}")]
    public async Task<ActionResult<List<GameInfo>>> SearchGameResults(string query) {
        try {
            var db = RedisConnector.db;
            if (db.KeyExists(query)) {
                var results = await db.StringGetAsync(query);
                string strVersion = results.ToString();
                List<GameInfo> allGameList = JsonConvert.DeserializeObject<List<GameInfo>>(strVersion)!;
                OkObjectResult res2 = new OkObjectResult(allGameList);
                return Ok(res2);
            }
            var searchResponse = await _client.SearchAsync<GameInfo>(s => s
                .Index("Game") // Specify the index name
                .Query(q => q
                    .Fuzzy(m => m
                        .Field(f => f.Matchup) // Specify the field to search
                        .Value(query) // Specify the search query
                        .Fuzziness(new Fuzziness(1))
                    )
                )
            );

            if (searchResponse.IsValidResponse)
            {
                var hits = searchResponse.Hits;

                var strList = JsonConvert.SerializeObject(hits);
                await db.StringSetAsync("all_games", strList, TimeSpan.FromSeconds(3600));

                OkObjectResult res = new OkObjectResult(hits);

                return Ok(res);
            }
            else
            {
                return BadRequest();
            }
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("League/{query}")]
    public async Task<ActionResult> AddLeague(Dictionary<string, object> reqBody) {
         try {
            var league = new LeagueInfo {
                Id = Guid.NewGuid().ToString(),
                LeagueName = reqBody["LeagueName"].ToString(),
                LeagueId = reqBody["LeagueId"].ToString()
            };

            await _client.IndexAsync(league, "League");

            return Ok();
        } catch {
            return BadRequest();
        }
    }


    [HttpDelete("League/{LeagueInfoId}")]
    public async Task<ActionResult> RemoveLeague(string LeagueInfoId) {
        try {
            await _client.DeleteAsync("League", LeagueInfoId);
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpGet("League")]
    public async Task<ActionResult<List<LeagueInfo>>> GetAllLeagues() {
        try {
            var db = RedisConnector.db;
            if (db.KeyExists("all_leagues")) {
                var results = await db.StringGetAsync("all_leagues");
                string strVersion = results.ToString();
                List<LeagueInfo> allGameList = JsonConvert.DeserializeObject<List<LeagueInfo>>(strVersion)!;
                OkObjectResult res2 = new OkObjectResult(allGameList);
                return Ok(res2);
            }
            var searchResponse = await _client.SearchAsync<LeagueInfo>(s => s
                .Index("League") 
                .Query(q => q
                    .MatchAll() 
                )
            );

            if (searchResponse.IsValidResponse) {
                var strList = JsonConvert.SerializeObject(searchResponse.Documents);
                await db.StringSetAsync("all_leagues", strList, TimeSpan.FromSeconds(3600));

                OkObjectResult res = new OkObjectResult(searchResponse.Documents);

                return Ok(res);
            }
            else {
                return BadRequest();
            }
        } catch {
            return BadRequest();
        }
    }


    [HttpGet("League/{query}")]
    public async Task<ActionResult<List<LeagueInfo>>> SearchLeagueResults(string query) {
        try {
            var db = RedisConnector.db;
            if (db.KeyExists(query)) {
                var results = await db.StringGetAsync(query);
                string strVersion = results.ToString();
                List<LeagueInfo> allGameList = JsonConvert.DeserializeObject<List<LeagueInfo>>(strVersion)!;
                OkObjectResult res2 = new OkObjectResult(allGameList);
                return Ok(res2);
            }
            var searchResponse = await _client.SearchAsync<LeagueInfo>(s => s
                .Index("League") // Specify the index name
                .Query(q => q
                    .Fuzzy(m => m
                        .Field(f => f.LeagueName) // Specify the field to search
                        .Value(query) // Specify the search query
                        .Fuzziness(new Fuzziness(1))
                    )
                )
            );

            if (searchResponse.IsValidResponse)
            {
                var hits = searchResponse.Hits;
                var strList = JsonConvert.SerializeObject(hits);
                await db.StringSetAsync("all_leagues", strList, TimeSpan.FromSeconds(3600));
                OkObjectResult res = new OkObjectResult(hits);

                return Ok(res);
            }
            else
            {
                return BadRequest();
            }
        } catch {
            return BadRequest();
        }
    }

}
