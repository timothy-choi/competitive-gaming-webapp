namespace CompetitiveGamingApp.Controllers;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;
using Elastic.Clients.Elasticsearch;
using StackExchange.Redis;
using Elastic.Transport;

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
            var searchResponse = await _client.SearchAsync<PlayerInfo>(s => s
                .Index("Player") 
                .Query(q => q
                    .MatchAll() 
                )
            );

            if (searchResponse.IsValidResponse) {
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

    [HttpGet("Player/{query}")]
    public async Task<ActionResult<List<string>>> SearchPlayerResults(string query) {
        try {
            var searchResponse = await _client.SearchAsync<LeagueInfo>(s => s
                .Index("your_index_name") // Specify the index name
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
            var searchResponse = await _client.SearchAsync<GameInfo>(s => s
                .Index("Game") 
                .Query(q => q
                    .MatchAll() 
                )
            );

            if (searchResponse.IsValidResponse) {
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
    public async Task<ActionResult<List<string>>> SearchGameResults(string query) {
        try {
            var searchResponse = await _client.SearchAsync<LeagueInfo>(s => s
                .Index("your_index_name") // Specify the index name
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
            var searchResponse = await _client.SearchAsync<LeagueInfo>(s => s
                .Index("League") 
                .Query(q => q
                    .MatchAll() 
                )
            );

            if (searchResponse.IsValidResponse) {
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
    public async Task<ActionResult<List<string>>> SearchLeagueResults(string query) {
        try {
            var searchResponse = await _client.SearchAsync<LeagueInfo>(s => s
                .Index("your_index_name") // Specify the index name
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
