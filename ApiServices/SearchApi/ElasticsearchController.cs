namespace CompetitiveGamingApp.Controllers;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using Microsoft.EntityFrameworkCore;
using Elasticsearch.Net;
using Nest;
using CompetitiveGamingApp;
using Newtonsoft.Json;
using ApiServices.SearchApi.RedisServer;

[ApiController]
[Route("api/Search")]

public class ElasticsearchController : ControllerBase {
    private readonly ElasticClient _client;
    public ElasticsearchController() {
           var apiKey = Environment.GetEnvironmentVariable("Elasticsearch_API_Key");
    var settings = new ConnectionSettings(new Uri("https://your-elasticsearch-url")) // Replace with your URL
        .ApiKeyAuthentication(new ApiKeyAuthenticationCredentials(apiKey));

        _client = new ElasticClient(settings);
        try {
            _client.Indices.Create("Player");
        } catch {}

        try {
            _client.Indices.Create("Game");
        } catch {}

        try {
            _client.Indices.Create("League");
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

            await _client.IndexDocumentAsync(player);

            return Ok();

        } catch {
            return BadRequest();
        }
    }

    [HttpDelete("Player/{PlayerInfoId}")]
    public async Task<ActionResult> RemovePlayer(string PlayerInfoId) {
        try {
            var deleteRequest = DocumentPath<PlayerInfo>.Id(PlayerInfoId)
    .Index("Player");
            
            await _client.DeleteAsync<PlayerInfo>(deleteRequest);
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

            if (searchResponse.IsValid) {
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
                        .Fuzziness(Fuzziness.Auto)
                    )
                )
            );
            if (searchResponse.IsValid) {
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
                        .Fuzziness(Fuzziness.Auto)
                    )
                )
            );

            if (searchResponse.IsValid)
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

            await _client.IndexDocumentAsync(game);

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpDelete("Game/{GameInfoId}")]
    public async Task<ActionResult> RemoveGame(string GameInfoId) {
        try {
            var deleteRequest = new DeleteRequest("Game", GameInfoId);
            await _client.DeleteAsync(deleteRequest);
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

            if (searchResponse.IsValid) {
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
                        .Fuzziness(Fuzziness.Auto)
                    )
                )
            );

            if (searchResponse.IsValid)
            {
                var hits = searchResponse.Hits;

                var strList = JsonConvert.SerializeObject(hits);
                await db.StringSetAsync(query, strList, TimeSpan.FromSeconds(3600));

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

            await _client.IndexAsync(league, i => i.Index("League"));

            return Ok();
        } catch {
            return BadRequest();
        }
    }


    [HttpDelete("League/{LeagueInfoId}")]
    public async Task<ActionResult> RemoveLeague(string LeagueInfoId) {
        try {
            var deleteRequest = new DeleteRequest("League", LeagueInfoId);
            await _client.DeleteAsync(deleteRequest);
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

            if (searchResponse.IsValid) {
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
                        .Fuzziness(Fuzziness.Auto)
                    )
                )
            );

            if (searchResponse.IsValid)
            {
                var hits = searchResponse.Hits;
                var strList = JsonConvert.SerializeObject(hits);
                await db.StringSetAsync(query, strList, TimeSpan.FromSeconds(3600));
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
