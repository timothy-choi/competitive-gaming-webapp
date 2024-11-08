using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Algolia.Search.Clients;
using Algolia.Search.Models.Search;
using CompetitiveGamingApp.Models;
using ApiServices.SearchApi.RedisServer;

namespace CompetitiveGamingApp.Controllers
{
    [ApiController]
    [Route("api/Search")]
    public class AlgoliaController : ControllerBase
    {
        private readonly SearchClient _client;

        public AlgoliaController()
        {
            _client = new SearchClient("W047O29WOY", "50f4c277555de8260d0d84b5e58bb844");
        }

        // Utility for caching
        private async Task CacheDataAsync<T>(string key, T data, TimeSpan expiration)
        {
            var db = RedisConnector.db;
            await db.StringSetAsync(key, JsonConvert.SerializeObject(data), expiration);
        }

        private async Task<List<T>> GetCachedDataAsync<T>(string key)
        {
            var db = RedisConnector.db;
            if (await db.KeyExistsAsync(key))
            {
                var cachedResults = await db.StringGetAsync(key);
                return JsonConvert.DeserializeObject<List<T>>(cachedResults);
            }
            return null;
        }

        // Player CRUD
        [HttpPost("Player")]
        public async Task<ActionResult> CreatePlayer(Dictionary<string, object> reqBody)
        {
            try
            {
                var player = new PlayerInfo
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = reqBody["Name"].ToString(),
                    Username = reqBody["Username"].ToString(),
                    PlayerId = reqBody["PlayerId"].ToString()
                };
                
                //var index = _client.InitIndex("Player");
                await _client.SaveObjectAsync("Player", player);
                return Ok("Player created successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating player: {ex.Message}");
            }
        }

        [HttpDelete("Player/{objectID}")]
        public async Task<ActionResult> DeletePlayer(string objectID)
        {
            try
            {
                //var index = _client.InitIndex("Player");
                await _client.DeleteObjectAsync("Player", objectID);
                return Ok("Player deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting player: {ex.Message}");
            }
        }

        // Game CRUD
        [HttpPost("Game")]
        public async Task<ActionResult> CreateGame(Dictionary<string, object> reqBody)
        {
            try
            {
                var game = new GameInfo {
                    Id = Guid.NewGuid().ToString(),
                    Matchup = reqBody["Matchup"].ToString(),
                    GameId = reqBody["GameId"].ToString()
                };

                //var index = _client.InitIndex("Game");
                await _client.SaveObjectAsync("Game", game);
                return Ok("Game created successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating game: {ex.Message}");
            }
        }

        [HttpDelete("Game/{objectID}")]
        public async Task<ActionResult> DeleteGame(string objectID)
        {
            try
            {
                //var index = _client.InitIndex("Game");
                await _client.DeleteObjectAsync("Game", objectID);
                return Ok("Game deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting game: {ex.Message}");
            }
        }

        // League CRUD
        [HttpPost("League")]
        public async Task<ActionResult> CreateLeague(Dictionary<string, object> reqBody)
        {
            try
            {
                var League = new LeagueInfo {
                    Id = Guid.NewGuid().ToString(),
                    LeagueName = reqBody["LeagueName"].ToString(),
                    LeagueId = reqBody["LeagueId"].ToString()
                };

                await _client.SaveObjectAsync("League", League);
                return Ok("League created successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating league: {ex.Message}");
            }
        }

        [HttpDelete("League/{objectID}")]
        public async Task<ActionResult> DeleteLeague(string objectID)
        {
            try
            {
                //var index = _client.InitIndex("League");
                await _client.DeleteObjectAsync("League", objectID);
                return Ok("League deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting league: {ex.Message}");
            }
        }

        // Search Endpoints
        [HttpGet("Player/Search/{query}")]
        public async Task<ActionResult> SearchPlayers(string query)
        {
            try
            {
                //var index = _client.InitIndex("Player");
                var searchResults = await _client.SearchAsync<PlayerInfo>(
                    new SearchMethodParams { 
                        Requests = new List<SearchQuery> {
                            new SearchQuery(new SearchForHits { IndexName = "Player", Query = query })
                        }
                    }
                    );

                return Ok(searchResults.ToJson());
            }
            catch (Exception ex)
            {
                return BadRequest($"Error searching players: {ex.Message}");
            }
        }

        [HttpGet("Game/Search/{query}")]
        public async Task<ActionResult> SearchGames(string query)
        {
            try
            {
                var searchResults = await _client.SearchAsync<GameInfo>(
                    new SearchMethodParams { 
                        Requests = new List<SearchQuery> {
                            new SearchQuery(new SearchForHits { IndexName = "Game", Query = query })
                        }
                    }
                    );

                return Ok(searchResults.ToJson());
            }
            catch (Exception ex)
            {
                return BadRequest($"Error searching games: {ex.Message}");
            }
        }

        [HttpGet("League/Search/{query}")]
        public async Task<ActionResult> SearchLeagues(string query)
        {
            try
            {
                var searchResults = await _client.SearchAsync<LeagueInfo>(
                    new SearchMethodParams { 
                        Requests = new List<SearchQuery> {
                            new SearchQuery(new SearchForHits { IndexName = "League", Query = query })
                        }
                    }
                    );

                return Ok(searchResults.ToJson());
            }
            catch (Exception ex)
            {
                return BadRequest($"Error searching leagues: {ex.Message}");
            }
        }
    }
}

