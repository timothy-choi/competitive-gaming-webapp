namespace CompetitiveGamingApp.Controller;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;

[ApiController]
[Route("api/League")]
public class LeagueController : ControllerBase {
    private readonly MongoDBService _leagueService;
    public LeagueController(MongoDBService leagueService) {
        _leagueService = leagueService;
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
    public async Task<ActionResult<string>> CreateLeague(Dictionary<string, object> leagueInput) {
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
            OkObjectResult res = new OkObjectResult(curr.LeagueId);
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
            Dictionary<String, String> body = new Dictionary<String, String>();
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
            Dictionary<String, String> body = new Dictionary<String, String>();
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
            Dictionary<String, String> body = new Dictionary<String, String>();
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
            Dictionary<String, String> body = new Dictionary<String, String>();
            body["tag"] = TagValue;

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["tag"] = true;
            await _leagueService.EditData("leagueInfo", upsertStatus, body);
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("{LeagueId}/players/{PlayerId}")]
    public async Task<ActionResult> AddNewPlayer(string LeagueId, string PlayerId) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }
            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["Players"] = true;

            Dictionary<string, string> playerInfo = new Dictionary<string, string>();
            playerInfo["PlayerId"] = PlayerId;
            playerInfo["DateJoined"] = DateTime.Now;
            Dictionary<String, object> body = new Dictionary<String, object>();
            body["Players"] = playerInfo;

            await _leagueService.EditData("leagueInfo", upsertStatus, body);
            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    [HttpPut("{LeagueId}/players/{PlayerId}/delete")]
    public async Task<ActionResult> RemovePlayer(string LeagueId, string PlayerId) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }
            var players = league.Players;

            int size = players.Count;

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["Players"] = false;

            players.RemoveAll(p => p.containsKey("PlayerId") && p["PlayerId"].ToString() == PlayerId);

            if (players.Count == size) {
                return NotFound();
            }

            Dictionary<string, object> playersVal = new Dictionary<string, object>();
            playersVal["Players"] = players;

            await _leagueService.EditData("leagueId", upsertStatus, playersVal);
            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    [HttpPut("{LeagueId}/Champion/{PlayerId}")]
    public async Task<ActionResult> AddNewChampion(string LeagueId, string PlayerId) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }

            var champions_size = league.Champions.Count;

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["Champions"] = true;

            Dictionary<string, object> champ = new Dictionary<string, object>();
            champ["Champions"] = Tuple.Create(PlayerId, "Season " + champions_size + 1);

            await _leagueService.EditData("leagueId", upsertStatus, champ);

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("{LeagueId}")]
    public async Task<ActionResult> ResetLeagueStandings(string LeagueId) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }

            var leagueStandings = league.LeagueStandings;
            leagueStandings.Season = league.Champion.Count + 1;
            leagueStandings.Table = leagueStandings.Table.OrderBy(d => d["playerName"]).ToList();
            for (int i = 0; i < leagueStandings.Table.Count; ++i) {
                for (var k in leagueStandings.Table[i]) {
                    if (k == "playerName" || typeof(leagueStandings.Table[i][k]) == typeof(string)) {
                        continue;
                    }
                    leagueStandings.Table[i][k] = 0;
                }
            }

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["LeagueStandings"] = true;

            Dictionary<string, object> leagueTable = new Dictionary<string, object>();
            leagueTable["LeagueStandings"] = leagueStandings;


            await _leagueService.EditData("leagueInfo", upsertStatus, leagueTable);
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
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("{LeagueId}/Division/Create")]
    public async Task<ActionResult> CreateDivisions(string LeagueId, Dictionary<string, object> reqBody) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }

            var divisions = reqBody["divisions"];

            Dictionary<string, object> divs = new Dictionary<string, object>();

            for (int i = 0; i < divisions.Count; ++i) {
                DivisionTable divTable = new DivisionTable {
                    DivisionTableId = Guid.NewGuid().ToString(),
                    DivisionName = divisions[i],
                    Season = league.Champion.Count + 1,
                    Table = new List<Dictionary<String, object>>()
                };
                divs[divisions[i]] = divTable;
            }

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["DivisionStandings"] = false;

            Dictionary<string, object> divStandings = new Dictionary<string, object>();
            divStandings["DivisionStandings"] = divs;

            await _leagueService.EditData("leagueInfo", upsertStatus, divStandings);

            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    [HttpPost("{LeagueId}/{DivisionName}/{PlayerId}")]
    public async Task<ActionResult> AddPlayerToDivision(string LeagueId, string DivisionName, string PlayerId, Dictionary<string, object> reqBody) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }

            var divisions = _leagueService.DivisionStandings;

            if (!divisions.containsKey(DivisionName)) {
                return NotFound();
            }

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["DivisionStandings." + DivisionName + ".Table"] = true;

            Dictionary<string, object> entry = new Dictionary<string, object>();
            entry[PlayerId] = reqBody;

            Dictionary<string, object> DivPlayer = new Dictionary<string, object>();
            DivPlayer["DivisionStandings." + DivisionName + ".Table"] = entry;

            await _leagueService.EditData("leagueInfo", upsertStatus, DivPlayer);

            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    [HttpPut("{LeagueId}/ResetDivisions")]
    public async Task<ActionResult> ResetDivisions(string LeagueId, Dictionary<string, object> reqBody) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }

            var divisions = league.DivisionStandings;

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["DivisionStandings"] = true;

            if (reqBody["ReassignEverySeason"]) {
                for (var k in reqBody) {
                    if (k == "ReassignEverySeason") {
                        continue;
                    }
                    divisions[k] = new DivisionTable();
                    divisions[k].DivisionName = k;
                    divisions[k].DivisionTableId = Guid.NewGuid().ToString(); 
                    divisions[k].Season = league.Champions.Count + 1;

                    divisions[k].Table = new List<Dictionary<string, object>>();

                    for (var player in reqBody[k]) {
                        divisions[k].Table.Push(player.Value);
                    }
                }
            }
            else {
                foreach (var div in divisions) {
                    var table = divisions[div];
                    divisions[div] = new DivisionTable();
                    divisions[div].DivisionName = div;
                    divisions[div].DivisionTableId = Guid.NewGuid().ToString();
                    divisions[div].Season = league.Champions.Count + 1;
                    divisions[div].Table = table.OrderBy(d => d["PlayerId"]).ToList();

                    foreach (var k in divisions[div].Table) {
                        foreach (var metric in divisions[div].Table[k]) {
                            if (typeof(divisions[div].Table[k]) == typeof(string)) {
                                continue;
                            }
                            divisions[div].Table[k] = 0;
                        }
                    }
                }
            }

            Dictionary<string, object> newDivisions = new Dictionary<string, object>();
            newDivisions["DivisionStandings"] = divisions;

            await _leagueService.EditData("leagueInfo", upsertStatus, newDivisions);

            return Ok();
        } catch {
            return BadRequest();
        }
    }


    [HttpPost("{LeagueId}/CombinedDivision")]
    public async Task<ActionResult> CreateCombinedDivision(string LeagueId, Dictionary<string, object> reqBody) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
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
                combTable.Divisions = combined.Value;
                combTable.Table = new List<Dictionary<string, object>>();
                allCombinedDivisions[combined.Key] = combTable;
            }

            Dictionary<string, object> totalCombDivisions = new Dictionary<string, object>();
            totalCombDivisions["CombinedDivisionStandings"] = allCombinedDivisions;

            await _leagueService.EditData("leagueInfo", upsertStatus, totalCombDivisions);

            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    [HttpPost("{LeagueId}/{PlayerId}/{DivisionName}/Combined")]
    public async Task<ActionResult> AddPlayerToCombinedStandings(string LeagueId, string playerId, string DivisionName, Dictionary<string, object> reqBody) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }

            var division = league.DivisionStandings[DivisionName];
            bool playerFound = false;
            for (int i = 0; i < division.Table.Count; ++i) {
                if (division.Table[i].containsKey(playerId)) {
                    playerFound = true;
                    break;
                }
            }

            if (league.CombinedDivisionStandings.containsKey(DivisionName) || !playerFound) {
                return NotFound();
            }

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["CombinedDivisionStandings." + DivisionName + ".Table"] = true;

            Dictionary<string, object> PlayerEntry = new Dictionary<string, object>();
            PlayerEntry["CombinedDivisionStandings." + DivisionName + ".Table"] = reqBody;

            await _leagueService.EditData("leagueInfo", upsertStatus, PlayerEntry);

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("{LeagueId}/CombinedDivision/Reset")]
    public async Task<ActionResult> ResetCombinedDivisionStandings(string LeagueId, Dictionary<string, object> reqBody) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }

            var combinedDivision = league.CombinedDivisionStandings;

            var copyCombinedDivision = league.CombinedDivisionStandings;

            Dictionary<string, object> allCombinedDivisions;


            if(reqBody["ReassignEverySeason"]) {
                for (var k in reqBody) {
                    if (typeof(reqBody[k]) == typeof(string)) {
                        continue;
                    }
                    combinedDivision[k] = new CombinedDivisionTable();
                    combinedDivision[k].CombinedDivisionTableId = Guid.NewGuid().ToString();
                    combinedDivision[k].CombinedDivisionName = k;
                    combinedDivision[k].Divisions = reqBody[k];
                    combinedDivision[k].Seasons = league.Champions.Count + 1;
                    combinedDivision[k].Table = new List<Dictionary<string, object>>();
                }
            }
            else {
                for (var combDiv in combinedDivision) {
                    var table = combinedDivision[combDiv];
                    combinedDivision[combDiv] = new CombinedDivisionTable();
                    combinedDivision[combDiv].CombinedDivisionTableId = Guid.NewGuid().ToString();
                    combinedDivision[combDiv].CombinedDivisionName = combDiv;
                    combinedDivision[combDiv].Seasons = league.Champions.Count + 1;
                    combinedDivision[combDiv].Divisions = copyCombinedDivision[combDiv].Divisions;
                    combinedDivision[combDiv].Table = table.OrderBy(d => d["PlayerId"]).ToList();

                    for (var k in combinedDivision[combDiv].Table) {
                        for (var metric in combinedDivision[combDiv].Table[k]) {
                            if (typeof(combinedDivision[combDiv].Table[k][metric]) == typeof(string)) {
                                continue;
                            }
                            divisions[div].Table[k][metric] = 0;
                        }
                    }
                }
            }

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["CombinedDivisionStandings"] = false;

            Dictionary<string, object> totalCombDivisions = new Dictionary<string, object>();
            totalCombDivisions["CombinedDivisionStandings"] = combinedDivision;


            await _leagueService.EditData("leagueInfo", upsertStatus, totalCombDivisions);

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
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("/Record/{LeagueId}")]
    public async Task<ActionResult> UpdatePlayerStandings(string LeagueId, Dictionary<string, object> reqBody) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
            if (league == null) {
                return NotFound();
            }

            var leagueStandings = league.LeagueStandings.Table;

            for (int i = 0; i < leagueStandings.Count; ++i) {
                if (leagueStandings[i]["playerId"] == reqBody["PlayerId"]) {
                    if (reqBody["recordStatus"] == 1) {
                        leagueStandings[i]["wins"] += 1;
                    }
                    else if (reqBody["recordStatus"] == -1) {
                        leagueStandings[i]["losses"] += 1;
                    }
                    else if (reqBody["recordStatus"] == 0) {
                        leagueStandings[i]["draws"] += 1;
                    }

                    for (var k in reqBody) {
                        if (k == "recordStatus" || k == "divisionName" || k == "combinedDivisionName") {
                            continue;
                        }
                        leagueStandings[i][k] = reqBody[k];
                    }
                    break;
                }
            }

            List<string> sortFactors = new List<string>();
            sortFactors.Add("wins");
            sortFactors.Add("losses");
            sortFactors.Add("draws");
            for (var k in reqBody) {
                if (k == "recordStatus" || k == "divisionName" || k == "combinedDivisionName") {
                    continue;
                }
                sortFactors.Add(k);
            }

            leagueStandings.Sort(new PlayerComparer(sortFactors));

            Dictionary<string, bool> upsertStatus = new Dictionary<string, bool>();
            upsertStatus["LeagueStandings.Table"] = false;

            Dictionary<string, object> updatedTable = new Dictionary<string, object>();
            updatedTable["LeagueStandings.Table"] = leagueStandings;


            await _leagueService.EditData("leagueInfo", upsertStatus, updatedTable);

            var division = league.Division["divisionName"].Table;

            for (int i = 0; i < division.Count; ++i) {
                if (division[i]["playerId"] == reqBody["PlayerId"]) {
                    if (reqBody["recordStatus"] == 1) {
                        division[i]["wins"] += 1;
                    }
                    else if (reqBody["recordStatus"] == -1) {
                        division[i]["losses"] += 1;
                    }
                    else if (reqBody["recordStatus"] == 0) {
                        division[i]["draws"] += 1;
                    }

                    foreach (var k in reqBody) {
                        if (k == "recordStatus" || k == "divisionName" || k == "combinedDivisionName") {
                            continue;
                        }
                        division[i][k] = reqBody[k];
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
                    if (reqBody["recordStatus"] == 1) {
                        combinedDivision[i]["wins"] += 1;
                    }
                    else if (reqBody["recordStatus"] == -1) {
                        combinedDivision[i]["losses"] += 1;
                    }
                    else if (reqBody["recordStatus"] == 0) {
                        combinedDivision[i]["draws"] += 1;
                    }

                    for (var k in reqBody) {
                        if (k == "recordStatus" || k == "divisionName" || k == "combinedDivisionName") {
                            continue;
                        }
                        combinedDivision[i][k] = reqBody[k];
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

            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    [HttpPut("{LeagueId}/ArchieveStandings")]
    public async Task<ActionResult> ArchieveStandings(string LeagueId) {
        try {
            var league = await _leagueService.GetData("leagueInfo", LeagueId);
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

}


