
namespace CompetitiveGamingApp.Controllers;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using static PlayerComparer;
using KafkaHelper;
using Newtonsoft.Json;

[ApiController]
[Route("api/League")]
public class LeagueController : ControllerBase {
    private readonly MongoDBService _leagueService;
    private readonly KafkaProducer _kafkaProducer;
    public LeagueController(MongoDBService leagueService) {
        _leagueService = leagueService;
        _kafkaProducer = new KafkaProducer();
    }

    [HttpGet]
    public async Task<ActionResult<List<League>>> GetAllLeagues() {
       var allLeagues = await _leagueService.GetAllData("leagueInfo");
       OkObjectResult res = new OkObjectResult(allLeagues);
       return Ok(allLeagues); 
    }

    [HttpGet("{LeagueId}")]
    public async Task<ActionResult<League>> GetLeagueById(string LeagueId) {
        var league = (League) await _leagueService.GetData("leagueInfo", LeagueId);
        if (league == null) {
            return NotFound();
        }
        OkObjectResult res = new OkObjectResult(league);
        return Ok(res);
    }

    [HttpPost]
    public async Task<ActionResult<League>> CreateLeague(Dictionary<string, object> leagueInput) {
        try {
            League curr = new League {
                LeagueId = Guid.NewGuid().ToString(),
                Name = leagueInput["LeagueName"].ToString(),
                Owner = leagueInput["LeagueOwner"].ToString(),
                Description = leagueInput["LeagueDescription"].ToString(),
                Players = new List<Dictionary<String, Object?>>(),
                tags = new List<string?>(),
                LeagueConfig = "",
                SeasonAssignments = "",
                LeagueStandings = new LeagueTable(),
                AchieveLeagueStandings = new List<LeagueTable>(),
                DivisionStandings = new Dictionary<string, DivisionTable>(),
                ArchieveDivisionStandings = new List<Dictionary<string, DivisionTable>>(),
                CombinedDivisionStandings = new Dictionary<string, CombinedDivisionTable>(),
                ArchieveCombinedDivisionStandings = new List<Dictionary<string, CombinedDivisionTable>>(),
                Champions = new List<Tuple<String, String>>(),
                PlayoffAssignments = ""
            };

            await _leagueService.PostData("leagueInfo", curr);
            OkObjectResult res = new OkObjectResult(curr);
            return Ok(res);
        }
        catch {
            return BadRequest();
        }
    }

    [HttpDelete("{LeagueId}")]
    public async Task<ActionResult> DeleteLeague(string LeagueId) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }
            await _leagueService.DeleteData("leagueInfo", LeagueId);
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("{LeagueId}/{SeasonAssignmentsId}")]
    public async Task<ActionResult> SetSeasonConfig(string LeagueId, string SeasonAssignmentsId) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }
            Dictionary<String, object> body = new Dictionary<String, object>();
            body["AssignmentsId"] = SeasonAssignmentsId;

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["AssignmentsId"] = false;
            await _leagueService.EditData("leagueInfo", upsertStatus, body);
            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    [HttpPut("{LeagueId}/{LeagueConfigId}")]
    public async Task<ActionResult> SetConfigId(string LeagueId, string LeagueConfigId) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }
            Dictionary<String, object> body = new Dictionary<String, object>();
            body["ConfigId"] = LeagueConfigId;

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["ConfigId"] = false;
            await _leagueService.EditData("leagueInfo", upsertStatus, body);
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("{LeagueId}/{PlayoffAssigmentId}")]
    public async Task<ActionResult> SetPlayoffAssignments(string LeagueId, string PlayoffAssignmentId) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }
            Dictionary<String, object> body = new Dictionary<String, object>();
            body["PlayoffAssignmentId"] = PlayoffAssignmentId;

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["PlayoffAssignmentId"] = false;
            await _leagueService.EditData("leagueInfo", upsertStatus, body);
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("{LeagueId}/tags/{TagValue}")] 
    public async Task<ActionResult> AddNewTag(string LeagueId, string TagValue) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }
            Dictionary<String, object> body = new Dictionary<String, object>();
            body["tag"] = TagValue;

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["tag"] = true;
            await _leagueService.EditData("leagueInfo", upsertStatus, body);
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("{LeagueId}/players/{PlayerName}")]
    public async Task<ActionResult> AddNewPlayer(string LeagueId, string PlayerName) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }
            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["Players"] = true;

            Dictionary<string, string> playerInfo = new Dictionary<string, string>();
            playerInfo["PlayerName"] = PlayerName;
            playerInfo["DateJoined"] = DateTime.Now.ToString();
            Dictionary<String, object> body = new Dictionary<String, object>();
            body["Players"] = playerInfo;

            await _leagueService.EditData("leagueInfo", upsertStatus, body);

            await _kafkaProducer.ProduceMessageAsync("AddingNewPlayerInLeague", PlayerName + "_" + playerInfo["DateJoined"], LeagueId);
            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    [HttpPut("{LeagueId}/players/{PlayerId}/delete")]
    public async Task<ActionResult> RemovePlayer(string LeagueId, string PlayerId) {
        try {
            var league = (League) await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }
            var players = league.Players;

            int size = players.Count;

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["Players"] = false;

            players.RemoveAll(p => p.ContainsKey("PlayerId") && p["PlayerId"].ToString() == PlayerId);

            if (players.Count == size) {
                return NotFound();
            }

            Dictionary<string, object> playersVal = new Dictionary<string, object>();
            playersVal["Players"] = players;

            await _leagueService.EditData("leagueId", upsertStatus, playersVal);

            await _kafkaProducer.ProduceMessageAsync("RemovingPlayerInLeague", PlayerId, LeagueId);
            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    [HttpPut("{LeagueId}/Champion/{PlayerName}")]
    public async Task<ActionResult> AddNewChampion(string LeagueId, string PlayerName) {
        try {
            var league = (League) await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }

            var champions_size = league.Champions.Count;

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["Champions"] = true;

            Dictionary<string, object> champ = new Dictionary<string, object>();
            champ["Champions"] = Tuple.Create(PlayerName, "Season " + champions_size + 1);

            await _leagueService.EditData("leagueId", upsertStatus, champ);

            await _kafkaProducer.ProduceMessageAsync("AddNewChampion", PlayerName, LeagueId);

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("{LeagueId}")]
    public async Task<ActionResult> ResetLeagueStandings(string LeagueId) {
        try {
            var league = (League) await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }

            var leagueStandings = league.LeagueStandings;
            leagueStandings.Season = league.Champions.Count + 1;
            leagueStandings.Table = leagueStandings.Table.OrderBy(d => d["playerName"]).ToList();
            for (int i = 0; i < leagueStandings.Table.Count; ++i) {
                foreach (var k in leagueStandings.Table[i]) {
                    if (k.Key == "playerName" || k.Value.GetType() == typeof(string)) {
                        continue;
                    }
                    leagueStandings.Table[i][k.Key] = 0;
                }
            }

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["LeagueStandings"] = true;

            Dictionary<string, object> leagueTable = new Dictionary<string, object>();
            leagueTable["LeagueStandings"] = leagueStandings;


            await _leagueService.EditData("leagueInfo", upsertStatus, leagueTable);

            await _kafkaProducer.ProduceMessageAsync("ResetLeagueStandings", JsonConvert.SerializeObject(leagueTable), LeagueId);
            return Ok();
        }
        catch {
            return BadRequest();
        }
    }


    [HttpPost("{LeagueId}/LeagueStandings")]
    public async Task<ActionResult> AddPlayerToLeagueStandings(string LeagueId, Dictionary<string, object> reqBody) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["LeagueStandings.Table"] = true;

            Dictionary<string, object> playerStandings = new Dictionary<string, object>();
            playerStandings["LeagueStandings.Table"] = reqBody;

            await _leagueService.EditData("leagueInfo", upsertStatus, playerStandings);

            await _kafkaProducer.ProduceMessageAsync("AddPlayerToLeague", JsonConvert.SerializeObject(reqBody), LeagueId);
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpDelete("{LeagueId}/{playerId}/LeagueStandings")]
    public async Task<ActionResult> DeletePlayerFromLeagueStandings(string LeagueId, string playerId) {
        try {
            var league = (League) await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["LeagueStandings.Table"] = false;

            var leagueStandings = league.LeagueStandings.Table;

            int index = -1;
            for (int i = 0; i < leagueStandings.Count; ++i) {
                if (leagueStandings[index]["playerId"] == playerId) {
                    index = i;
                    break;
                }
            }

            if (index == -1) {
                return NotFound();
            }

            leagueStandings.RemoveAt(index);

            Dictionary<string, object> playerStandings = new Dictionary<string, object>();
            playerStandings["LeagueStandings.Table"] = leagueStandings;

            await _leagueService.EditData("leagueInfo", upsertStatus, playerStandings);

            await _kafkaProducer.ProduceMessageAsync("RemovePlayerToLeague", playerId, LeagueId);
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpGet("{LeagueId}/{playerId}")]
    public async Task<ActionResult<Dictionary<string, object>>> GetPlayer(string LeagueId, string playerId) {
        var league = (League) await _leagueService.GetData("leagueInfo", LeagueId);
        if (league == null) {
            return NotFound();
        }

        var standings = league.LeagueStandings!.Table;

        foreach (var entry in standings!) {
            if (entry["playerId"].ToString() == playerId) {
                OkObjectResult res = new OkObjectResult(entry.Values);
                return Ok(res);
            }
        }

        return NotFound();
    }
 
    [HttpPost("{LeagueId}/Division/Create")]
    public async Task<ActionResult> CreateDivisions(string LeagueId, Dictionary<string, object> reqBody) {
        try {
            var league = (League) await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }

            var divisions = (List<String>) reqBody["divisions"];

            Dictionary<string, object> divs = new Dictionary<string, object>();

            for (int i = 0; i < divisions.Count; ++i) {
                DivisionTable divTable = new DivisionTable {
                    DivisionTableId = Guid.NewGuid().ToString(),
                    DivisionName = divisions[i],
                    Season = league.Champions.Count + 1,
                    Table = new List<Dictionary<String, object>>()
                };
                divs[divisions[i]] = divTable;
            }

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["DivisionStandings"] = false;

            Dictionary<string, object> divStandings = new Dictionary<string, object>();
            divStandings["DivisionStandings"] = divs;

            await _leagueService.EditData("leagueInfo", upsertStatus, divStandings);

            await _kafkaProducer.ProduceMessageAsync("CreateDivisions", JsonConvert.SerializeObject(divs), LeagueId);

            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    [HttpPost("{LeagueId}/{DivisionName}/{PlayerId}")]
    public async Task<ActionResult> AddPlayerToDivision(string LeagueId, string DivisionName, string PlayerId, Dictionary<string, object> reqBody) {
        try {
            var league = (League) await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }

            var divisions = league.DivisionStandings;

            if (!divisions.ContainsKey(DivisionName)) {
                return NotFound();
            }

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["DivisionStandings." + DivisionName + ".Table"] = true;

            Dictionary<string, object> entry = new Dictionary<string, object>();
            entry[PlayerId] = reqBody;

            Dictionary<string, object> DivPlayer = new Dictionary<string, object>();
            DivPlayer["DivisionStandings." + DivisionName + ".Table"] = entry;

            await _leagueService.EditData("leagueInfo", upsertStatus, DivPlayer);

            await _kafkaProducer.ProduceMessageAsync("AddPlayerToDivison", JsonConvert.SerializeObject(entry), LeagueId);

            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    [HttpDelete("{LeagueId}/{PlayerId}/Division")]
    public async Task<ActionResult> RemovePlayerFromDivision(string LeagueId, string PlayerId) {
        try {
            var league = (League) await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }

            string found_division = "";

            var divisions = league.DivisionStandings;
            foreach (var division in divisions) {
                if (division.Value.Table.Find(dict => dict["playerId"] == PlayerId).ToList().Count() > 0) {
                    found_division = division.Key;
                    break;
                }
            }

            if (found_division == "") {
                return NotFound();
            }

            divisions[found_division].Table.RemoveAll(entry => entry["playerId"] == PlayerId);

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["DivisionStandings." + found_division + ".Table"] = false;

            Dictionary<string, object> DivPlayer = new Dictionary<string, object>();
            DivPlayer["DivisionStandings." + found_division + ".Table"] = divisions[found_division].Table;

            await _leagueService.EditData("leagueInfo", upsertStatus, DivPlayer);

            await _kafkaProducer.ProduceMessageAsync("DeletePlayerFromDivisionStandings", JsonConvert.SerializeObject(divisions[found_division].Table), LeagueId);
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("{LeagueId}/ResetDivisions")]
    public async Task<ActionResult> ResetDivisions(string LeagueId, Dictionary<string, object> reqBody) {
        try {
            var league = (League) await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }

            var divisions = league.DivisionStandings;

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["DivisionStandings"] = true;

            if ((bool) reqBody["ReassignEverySeason"]) {
                foreach (var k in reqBody) {
                    if (k.Key == "ReassignEverySeason") {
                        continue;
                    }
                    divisions[k.Key] = new DivisionTable();
                    divisions[k.Key].DivisionName = k.Key;
                    divisions[k.Key].DivisionTableId = Guid.NewGuid().ToString(); 
                    divisions[k.Key].Season = league.Champions.Count + 1;

                    divisions[k.Key].Table = new List<Dictionary<string, object>>();

                    foreach (var player in (List<Dictionary<string, object>>) reqBody[k.Key]) {
                        divisions[k.Key].Table.Add(player);
                    }
                }
            }
            else {
                foreach (var div in divisions) {
                    var table = divisions[div.Key];
                    divisions[div.Key] = new DivisionTable();
                    divisions[div.Key].DivisionName = div.Key;
                    divisions[div.Key].DivisionTableId = Guid.NewGuid().ToString();
                    divisions[div.Key].Season = league.Champions.Count + 1;
                    divisions[div.Key].Table = table.Table.OrderBy(d => d["PlayerId"].ToString()).ToList();

                    for (int k = 0; k < divisions[div.Key].Table.Count; ++k) {
                        foreach (var metric in divisions[div.Key].Table[k]) {
                            if (metric.Value.GetType() == typeof(string)) {
                                continue;
                            }
                            divisions[div.Key].Table[k][metric.Key] = 0;
                        }
                    }
                }
            }

            Dictionary<string, object> newDivisions = new Dictionary<string, object>();
            newDivisions["DivisionStandings"] = divisions;

            await _leagueService.EditData("leagueInfo", upsertStatus, newDivisions);

            await _kafkaProducer.ProduceMessageAsync("ResetDivisions", JsonConvert.SerializeObject(divisions), LeagueId);

            return Ok();
        } catch {
            return BadRequest();
        }
    }


    [HttpPost("{LeagueId}/CombinedDivision")]
    public async Task<ActionResult> CreateCombinedDivision(string LeagueId, Dictionary<string, object> reqBody) {
        try {
            var league = (League) await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["CombinedDivisionStandings"] = false;

            Dictionary<string, object> allCombinedDivisions = new Dictionary<string, object>();

            foreach (var combined in reqBody) {
                CombinedDivisionTable combTable = new CombinedDivisionTable();
                combTable.CombinedDivisionName = combined.Key;
                combTable.CombinedDivisionTableId = Guid.NewGuid().ToString();
                combTable.Season = league.Champions.Count + 1;
                combTable.Divisions = (List<String>) combined.Value;
                combTable.Table = new List<Dictionary<string, object>>();
                allCombinedDivisions[combined.Key] = combTable;
            }

            Dictionary<string, object> totalCombDivisions = new Dictionary<string, object>();
            totalCombDivisions["CombinedDivisionStandings"] = allCombinedDivisions;

            await _leagueService.EditData("leagueInfo", upsertStatus, totalCombDivisions);

            await _kafkaProducer.ProduceMessageAsync("ResetDivisions", JsonConvert.SerializeObject(allCombinedDivisions), LeagueId);

            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    [HttpPost("{LeagueId}/{PlayerId}/{DivisionName}/Combined")]
    public async Task<ActionResult> AddPlayerToCombinedStandings(string LeagueId, string playerId, string DivisionName, Dictionary<string, object> reqBody) {
        try {
            var league = (League) await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }

            var division = league.DivisionStandings[DivisionName];
            bool playerFound = false;
            for (int i = 0; i < division.Table.Count; ++i) {
                if (division.Table[i].ContainsKey(playerId)) {
                    playerFound = true;
                    break;
                }
            }

            if (league.CombinedDivisionStandings.ContainsKey(DivisionName) || !playerFound) {
                return NotFound();
            }

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["CombinedDivisionStandings." + DivisionName + ".Table"] = true;

            Dictionary<string, object> PlayerEntry = new Dictionary<string, object>();
            PlayerEntry["CombinedDivisionStandings." + DivisionName + ".Table"] = reqBody;

            await _leagueService.EditData("leagueInfo", upsertStatus, PlayerEntry);

            await _kafkaProducer.ProduceMessageAsync("ResetDivisions", JsonConvert.SerializeObject(reqBody), LeagueId);

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpDelete("{LeagueId}/{PlayerId}/CombinedDivision")]
    public async Task<ActionResult> RemovePlayerFromCombinedDivision(string LeagueId, string PlayerId) {
        try {
            var league = (League) await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }

            string found_combined_division = "";

            var combDivisions = league.CombinedDivisionStandings;
            foreach (var combDivision in combDivisions) {
                if (combDivision.Value.Table.Find(dict => dict["playerId"] == PlayerId).ToList().Count() > 0) {
                    found_combined_division = combDivision.Key;
                    break;
                }
            }

            if (found_combined_division == "") {
                return NotFound();
            }

            combDivisions[found_combined_division].Table.RemoveAll(entry => entry["playerId"] == PlayerId);

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["DivisionStandings." + found_combined_division + ".Table"] = false;

            Dictionary<string, object> DivPlayer = new Dictionary<string, object>();
            DivPlayer["DivisionStandings." + found_combined_division + ".Table"] = combDivisions[found_combined_division].Table;

            await _leagueService.EditData("leagueInfo", upsertStatus, DivPlayer);

            await _kafkaProducer.ProduceMessageAsync("ResetDivisions", JsonConvert.SerializeObject(combDivisions[found_combined_division].Table), LeagueId);

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("{LeagueId}/CombinedDivision/Reset")]
    public async Task<ActionResult> ResetCombinedDivisionStandings(string LeagueId, Dictionary<string, object> reqBody) {
        try {
            var league = (League) await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }

            var combinedDivision = league.CombinedDivisionStandings;

            var copyCombinedDivision = league.CombinedDivisionStandings;

            Dictionary<string, object> allCombinedDivisions = new Dictionary<string, object>();


            if((bool) reqBody["ReassignEverySeason"]) {
                foreach (var k in reqBody) {
                    if (k.Value.GetType() == typeof(string)) {
                        continue;
                    }
                    combinedDivision[k.Key] = new CombinedDivisionTable();
                    combinedDivision[k.Key].CombinedDivisionTableId = Guid.NewGuid().ToString();
                    combinedDivision[k.Key].CombinedDivisionName = k.Key;
                    combinedDivision[k.Key].Divisions = (List<String>) k.Value;
                    combinedDivision[k.Key].Season = league.Champions.Count + 1;
                    combinedDivision[k.Key].Table = new List<Dictionary<string, object>>();
                }
            }
            else {
                foreach (var combDiv in combinedDivision) {
                    var table = combinedDivision[combDiv.Key];
                    combinedDivision[combDiv.Key] = new CombinedDivisionTable();
                    combinedDivision[combDiv.Key].CombinedDivisionTableId = Guid.NewGuid().ToString();
                    combinedDivision[combDiv.Key].CombinedDivisionName = combDiv.Key;
                    combinedDivision[combDiv.Key].Season = league.Champions.Count + 1;
                    combinedDivision[combDiv.Key].Divisions = copyCombinedDivision[combDiv.Key].Divisions;
                    combinedDivision[combDiv.Key].Table = table.Table.OrderBy(d => d["PlayerId"]).ToList();

                    for (int k = 0; k < combinedDivision[combDiv.Key].Table.Count; ++k) {
                        foreach (var metric in combinedDivision[combDiv.Key].Table[k]) {
                            if (combinedDivision[combDiv.Key].Table[k][metric.Key].GetType() == typeof(string)) {
                                continue;
                            }
                            combinedDivision[combDiv.Key].Table[k][metric.Key] = 0;
                        }
                    }
                }
            }

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["CombinedDivisionStandings"] = false;

            Dictionary<string, object> totalCombDivisions = new Dictionary<string, object>();
            totalCombDivisions["CombinedDivisionStandings"] = combinedDivision;


            await _leagueService.EditData("leagueInfo", upsertStatus, totalCombDivisions);

            await _kafkaProducer.ProduceMessageAsync("ResetCombinedDivisions", JsonConvert.SerializeObject(combinedDivision), LeagueId);

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("{LeagueId}/{DivisionId}/Combined/Rest/Add")]
    public async Task<ActionResult> AddPlayersToNewCombinedStandings(string LeagueId, string CombinedDivisionName, Dictionary<string, object> reqBody) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["CombinedDivisionStandings." + CombinedDivisionName + ".Table"] = true;

            Dictionary<string, object> totalCombDivisions = new Dictionary<string, object>();
            totalCombDivisions["CombinedDivisionStandings." + CombinedDivisionName + ".Table"] = reqBody;

            await _leagueService.EditData("leagueId", upsertStatus, totalCombDivisions);

            await _kafkaProducer.ProduceMessageAsync("AddToCombinedDivisions", JsonConvert.SerializeObject(reqBody), LeagueId);

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("/Record/{LeagueId}")]
    public async Task<ActionResult> UpdatePlayerStandings(string LeagueId, Dictionary<string, object> reqBody) {
        try {
            var league = (League) await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }

            var leagueStandings = league.LeagueStandings.Table;

            for (int i = 0; i < leagueStandings.Count; ++i) {
                if (leagueStandings[i]["playerId"] == reqBody["PlayerId"]) {
                    if ((int) reqBody["recordStatus"] == 1) {
                        int wins = Convert.ToInt32(leagueStandings[i]["wins"]);
                        leagueStandings[i]["wins"] = wins + 1;
                    }
                    else if ((int) reqBody["recordStatus"] == -1) {
                        int losses = Convert.ToInt32(leagueStandings[i]["losses"]);
                        leagueStandings[i]["losses"] = losses + 1;
                    }
                    else if ((int) reqBody["recordStatus"] == 0) {
                        int draws = Convert.ToInt32(leagueStandings[i]["draws"]);
                        leagueStandings[i]["draws"] = draws + 1;
                    }

                    foreach (var k in reqBody) {
                        if (k.Key == "recordStatus" || k.Key == "divisionName" || k.Key == "combinedDivisionName") {
                            continue;
                        }
                        leagueStandings[i][k.Key] = reqBody[k.Key];
                    }
                    break;
                }
            }

            List<string> sortFactors = new List<string>();
            sortFactors.Add("wins");
            sortFactors.Add("losses");
            sortFactors.Add("draws");
            foreach (var k in reqBody) {
                if (k.Key == "recordStatus" || k.Key == "divisionName" || k.Key == "combinedDivisionName") {
                    continue;
                }
                sortFactors.Add(k.Key);
            }

            leagueStandings.Sort(new PlayerComparer(sortFactors));

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["LeagueStandings.Table"] = false;

            Dictionary<string, object> updatedTable = new Dictionary<string, object>();
            updatedTable["LeagueStandings.Table"] = leagueStandings;


            await _leagueService.EditData("leagueInfo", upsertStatus, updatedTable);

            var division = league.DivisionStandings["divisionName"].Table;

            for (int i = 0; i < division.Count; ++i) {
                if (division[i]["playerId"] == reqBody["PlayerId"]) {
                    if ((int) reqBody["recordStatus"] == 1) {
                        int wins = Convert.ToInt32(division[i]["wins"]);
                        division[i]["wins"] = wins + 1;
                    }
                    else if ((int) reqBody["recordStatus"] == -1) {
                        int losses = Convert.ToInt32(division[i]["losses"]);
                        division[i]["losses"] = losses + 1;
                    }
                    else if ((int) reqBody["recordStatus"] == 0) {
                        int draws = Convert.ToInt32(division[i]["draws"]);
                        division[i]["draws"] = draws + 1;
                    }

                    foreach (var k in reqBody) {
                        if (k.Key == "recordStatus" || k.Key == "divisionName" || k.Key == "combinedDivisionName") {
                            continue;
                        }
                        division[i][k.Key] = reqBody[k.Key];
                    }
                    break;
                }
            }

            division.Sort(new PlayerComparer(sortFactors));

            Dictionary<string, bool> upsertStatusDivisions = new Dictionary<string, bool>();
            upsertStatusDivisions["DivisionStandings.Table"] = false;

            Dictionary<string, object> updatedDivisions = new Dictionary<string, object>();
            updatedDivisions["DivisionStandings.Table"] = division;


            await _leagueService.EditData("leagueInfo", upsertStatusDivisions, updatedDivisions);

            var combinedDivision = league.CombinedDivisionStandings["combinedDivisionName"].Table;

            for (int i = 0; i < combinedDivision.Count; ++i) {
                if (combinedDivision[i]["playerId"] == reqBody["PlayerId"]) {
                    if ((int) reqBody["recordStatus"] == 1) {
                        int wins = Convert.ToInt32(combinedDivision[i]["wins"]);
                        combinedDivision[i]["wins"]  = wins + 1;
                    }
                    else if ((int) reqBody["recordStatus"] == -1) {
                        int losses = Convert.ToInt32(combinedDivision[i]["losses"]);
                        combinedDivision[i]["losses"] = losses + 1;
                    }
                    else if ((int) reqBody["recordStatus"] == 0) {
                        int draws = Convert.ToInt32(combinedDivision[i]["draws"]);
                        combinedDivision[i]["draws"] = draws + 1;
                    }

                    foreach (var k in reqBody) {
                        if (k.Key == "recordStatus" || k.Key == "divisionName" || k.Key == "combinedDivisionName") {
                            continue;
                        }
                        combinedDivision[i][k.Key] = reqBody[k.Key];
                    }
                    break;
                }
            }

            combinedDivision.Sort(new PlayerComparer(sortFactors));

            Dictionary<string, bool> upsertStatusComb = new Dictionary<string, bool>();
            upsertStatusComb["DivisionStandings.Table"] = false;

            Dictionary<string, object> updatedComb = new Dictionary<string, object>();
            updatedComb["DivisionStandings.Table"] = combinedDivision;

            await _leagueService.EditData("leagueInfo", upsertStatusComb, updatedComb);

            var allStandings = new List<string> {JsonConvert.SerializeObject(leagueStandings), JsonConvert.SerializeObject(division), JsonConvert.SerializeObject(combinedDivision)};

            await _kafkaProducer.ProduceMessageAsync("UpdateStandings", string.Join(",", allStandings), LeagueId);

            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    [HttpPut("{LeagueId}/ArchieveStandings")]
    public async Task<ActionResult> ArchieveStandings(string LeagueId) {
        try {
            var league = (League) await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }

            Dictionary<string, bool> upsertOpt = new Dictionary<string, bool>();
            upsertOpt["ArchieveLeagueStandings"] = true;
            upsertOpt["ArchieveDivisionStandings"] = true;
            upsertOpt["ArchieveCombinedDivisionStandings"] = true;
            
            Dictionary<string, object> archievedTables = new Dictionary<string, object>();
            archievedTables["ArchieveLeagueStandings"] = league.LeagueStandings;
            archievedTables["ArchieveDivisionStandings"] = league.DivisionStandings;
            archievedTables["ArchieveCombinedDivisionStandings"] = league.CombinedDivisionStandings;

            await _leagueService.EditData("leagueInfo", upsertOpt, archievedTables);

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    //Adding just-in-case endpoint
    [HttpPost("{LeagueId}/RandomizePartialSelection")]
    public async Task<ActionResult<List<Dictionary<string, object>>>> RandomizePartialSelection(string LeagueId, Dictionary<string, object> reqBody) {
        try {
            var league = (League) await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }

            Random rm = new Random();

            List<Dictionary<string, object>> randList = (List<Dictionary<string, object>>) reqBody["conflictingPlayers"];

            randList = randList.OrderBy(_ => rm.Next()).ToList();

            OkObjectResult res = new OkObjectResult(randList);

            return Ok(res);
        } catch {
            return BadRequest();
        }
    }

}


