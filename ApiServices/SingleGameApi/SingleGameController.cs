namespace CompetitiveGamingApp.Controllers;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using System.Numerics;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using AWSHelper;
using RabbitMQ;
using KafkaHelper;

[ApiController]
[Route("api/singleGame")]

public class SingleGameController : ControllerBase {

    private readonly HttpClient client;
    private readonly SingleGameServices _singleGameService;
    private readonly Producer _producer;
    private readonly KafkaProducer _kafkaProducer;
    public SingleGameController(SingleGameServices singleGameServices) {
        _singleGameService = singleGameServices;
        client = new HttpClient();
        _producer = new Producer();
        _kafkaProducer = new KafkaProducer();
    }

    [HttpGet]
    public async Task<ActionResult<List<SingleGame>>> getAllGames() {
        List<SingleGame>? allGames = await _singleGameService.GetAllGames();
        OkObjectResult res = new OkObjectResult(allGames);
        return Ok(res);
    }

    [HttpGet("{gameId}")]
    public async Task<ActionResult<SingleGame>> getGame(string gameId) {
        try {
            SingleGame? game = await _singleGameService.GetGame(gameId);
            if (game == null) {
                return NotFound();
            }
            OkObjectResult res = new OkObjectResult(game);
            return Ok(res);
        } catch {
            return BadRequest();
        }
    }

    [HttpGet("{player}")]
    public async Task<ActionResult<List<SingleGame>>> GetAllGamesFromPlayer(string player) {
        List<SingleGame>? allGames = await _singleGameService.GetAllGames();
        List<SingleGame> playerGames = new List<SingleGame>();
        foreach (var game in allGames!) {
            if (game.guestPlayer == player || game.hostPlayer == player) {
                playerGames.Add(game);
            }
        }

        OkObjectResult res = new OkObjectResult(player);

        return Ok(res);
    }

    [HttpPost]
    public async Task<ActionResult<string>> createNewGame(Dictionary<string, object> gameInfo) {
        try {
            SingleGame scheduledGame = new SingleGame {
                SingleGameId = Guid.NewGuid().ToString(),
                hostPlayer = gameInfo["hostPlayer"].ToString(),
                guestPlayer = gameInfo["guestPlayer"].ToString(),
                hostScore = 0,
                guestScore = 0,
                timePlayed = TimeSpan.Parse(gameInfo["gametime"].ToString()),
                gameEditor = null,
                twitchBroadcasterId = null,
                predictionId = "",
                videoFilePath = ""
            };

            await _singleGameService.CreateGame(scheduledGame);

            OkObjectResult res = new OkObjectResult(scheduledGame.SingleGameId);

            return Ok(res);
        }
        catch (Exception e) {
            Console.WriteLine(e.Message);
            return BadRequest();
        }
    }

    [HttpDelete("{gameId}")]
    public async Task<ActionResult> deleteGame(string gameId) {
        try {
            SingleGame? game = await _singleGameService.GetGame(gameId);
            if (game == null) {
                return NotFound();
            }

            await _singleGameService.DeleteGame(gameId);

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("/Season")]
    public async Task<ActionResult> addSeasonGame([FromBody] Dictionary<string, object> gameInfo) {
        try {
            await _singleGameService.CreateGame((SingleGame) gameInfo["SeasonGame"]);
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("/finalScore")]
    public async Task<ActionResult> addFinalScore([FromBody] Dictionary<string, string> finalScoreInfo) {
        try {
            await _singleGameService.UpdateFinalScore(Convert.ToInt32(finalScoreInfo["hostScore"]), Convert.ToInt32(finalScoreInfo["guestScore"]), finalScoreInfo["gameId"]);

            var stringScore = $"{finalScoreInfo["hostScore"]},{finalScoreInfo["guestScore"]}";

            await _kafkaProducer.ProduceMessageAsync("UpdateSingleGameFinalScore", finalScoreInfo["gameId"] + "_" + stringScore, "app");
            await _kafkaProducer.ProduceMessageAsync("UpdateSingleGameFinalScore", stringScore, finalScoreInfo["gameId"]);

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("/editor/{gameId}/{editor}")]
    public async Task<ActionResult> updateGameEditor(string gameId, string editor) {
        try {
            var player = await client.GetAsync("/player/" + editor);
            if (player == null) {
                return NotFound();
            }
            await _singleGameService.EditUserGameEditor(editor, gameId);
            await _kafkaProducer.ProduceMessageAsync("AddInEditor", editor, gameId);
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("/otherGameInfo")]
    public async Task<ActionResult> AddOtherGameInfo([FromBody] Dictionary<string, string> otherGameInfo) {
        try {
            Dictionary<string, string> parsedGameInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(otherGameInfo["gameInfo"])!; 
            await _singleGameService.AddOtherGameInfo(parsedGameInfo!, otherGameInfo["gameId"]);

            string jsonString = JsonConvert.SerializeObject(parsedGameInfo);

            await _kafkaProducer.ProduceMessageAsync("AddOtherGameInfo", jsonString, otherGameInfo["gameId"]);
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("/twitchId/{broadcasterId}/{gameId}")]
    public async Task<ActionResult> AddTwitchId(string broadcasterId, string gameId) {
        try {
            var user = await client.GetAsync("https://api.twitch.tv/helix/users?id=" + broadcasterId);
            if ((int) user.StatusCode != 200) {
                return NotFound();
            }

            await _singleGameService.AddTwitchBroadcasterId(broadcasterId, gameId);
            await _kafkaProducer.ProduceMessageAsync("AddTwitchId", broadcasterId, gameId);
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("/twitchId/prediction")]
    public async Task<ActionResult<String>> CreatePrediction([FromBody] Dictionary<String, String> predictionInfo) {
        try {
            var stringContent = new StringContent(JsonConvert.SerializeObject(predictionInfo), Encoding.UTF8, "application/json");
            HttpResponseMessage res = await client.PostAsync("https://api.twitch.tv/helix/predictions", stringContent);
            if (!res.IsSuccessStatusCode) {
                return BadRequest();
            }
            string predData = await res.Content.ReadAsStringAsync();

            var predObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(predData)!;

            var data = (List<object>)predObj["data"];
            var firstDataElement = (Dictionary<string, object>)data[0];
            var id = (string)firstDataElement["id"];

            await _singleGameService.AddPredictionId(id, predictionInfo["gameId"]);

            await _kafkaProducer.ProduceMessageAsync("CreatePrediction", predData, predictionInfo["gameId"]);
            OkObjectResult newPred = new OkObjectResult(predData);
            return Ok(newPred);
        }
        catch {
            return BadRequest();
        }
    }

    [HttpGet("/twitchId/prediction/{broadcasterId}/{predId}")]
    public async Task<ActionResult<Dictionary<String, String>>> GetPrediction(string broadcasterId, string predId) {
        try {
            var res = await client.GetAsync("https://api.twitch.tv/helix/predictions?broadcaster_id=" + broadcasterId);
            if (!res.IsSuccessStatusCode) {
                return BadRequest();
            }
            var body = await res.Content.ReadAsStringAsync();
            var predList = JsonConvert.DeserializeObject<List<Dictionary<String, object>>>(body);
            for (int i = 0; i < predList!.Count; ++i) {
                if ((string) predList[i]["id"] == predId) {
                    OkObjectResult pred = new OkObjectResult(predList[i]);
                    return Ok(pred);
                }
            }
            return NotFound();
        } catch {
            return BadRequest();
        }
    }

    [HttpPatch("/twitchId/prediction/end")]
    public async Task<ActionResult<String>> EndPrediction([FromBody] Dictionary<String, String> endPredInfo) {
        try {
            var stringContent = new StringContent(JsonConvert.SerializeObject(endPredInfo), Encoding.UTF8, "application/json");
            HttpResponseMessage res = await client.PatchAsync("https://api.twitch.tv/helix/predictions", stringContent);
            if (!res.IsSuccessStatusCode) {
                return BadRequest();
            }
            var dataBody = await res.Content.ReadAsStringAsync();

            await _kafkaProducer.ProduceMessageAsync("EndPrediction", "ended", endPredInfo["gameId"]);
            OkObjectResult resolvedPred = new OkObjectResult(dataBody);
            return Ok(resolvedPred);
        } catch {
            return BadRequest();
        }
    }

    [HttpGet("/twitchId/stream/{broadcasterId}/{streamId}")]
    public async Task<ActionResult<Dictionary<String, String>>> GetStream(string broadcasterId, string streamId) {
        try {
            var res = await client.GetAsync("https://api.twitch.tv/helix/streams?user_id=" + broadcasterId + "&type=live");
            if (!res.IsSuccessStatusCode) {
                return NotFound();
            }
            var allStreamsStr = await res.Content.ReadAsStringAsync();
            var allStreams = JsonConvert.DeserializeObject<List<Dictionary<String, String>>>(allStreamsStr)!;
            List<Dictionary<String, String>> parsedStreams = new List<Dictionary<String, String>>();
            for (int x = 0; x < allStreams.Count; ++x) {
                var jsonObj = JObject.Parse(allStreams[x]["data"]);
                parsedStreams.Add(jsonObj.ToObject<Dictionary<String, String>>()!);
            }
            for (int i = 0; i < parsedStreams.Count; ++i) {
                if (parsedStreams[i]["id"] == streamId) {
                    parsedStreams[i]["stream_url"] = "http://twitch.tv/" + parsedStreams[i]["user_name"].ToLower();
                    OkObjectResult currStream = new OkObjectResult(parsedStreams[i]);
                    return Ok(currStream);
                }
            }
            return NotFound();
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("/twitchId/stream/{broadcasterId}/send")]
    public async Task<ActionResult> SendStreamLink(string broadcasterId, Dictionary<string, string> reqBody) {
        try {
            await _kafkaProducer.ProduceMessageAsync("SendStream", JsonConvert.SerializeObject(reqBody), broadcasterId);
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("/processRecording")]
    public async Task<ActionResult> ProcessGameRecording([FromBody] Dictionary<String, String> recordingInfo) {
        try {
            var res = await client.GetAsync("https://api.twitch.tv/helix/videos?user_id=" + recordingInfo["user_id"] + "&period=day&sort=time&type=archive");
            if (res == null) {
                return NotFound();
            }
            var videosStr = await res.Content.ReadAsStringAsync();
            var videos = JsonConvert.DeserializeObject<List<Dictionary<String, String>>>(videosStr)!;
            Dictionary<String, String> selectedVideo = new Dictionary<String, String>();
            int i = 0;
            for (i = 0; i < videos.Count; ++i) {
                if (recordingInfo["title"] == videos[i]["title"]) {
                    selectedVideo = videos[i];
                    break;
                }
            }

            if (i == videos.Count) {
                return NotFound();
            }

            string cmd = "twitch-dl download ";
            string args = selectedVideo["url"] + " --output " + selectedVideo["title"] + ".mov";

            ProcessStartInfo psi = new ProcessStartInfo {
                FileName = "/bin/bash",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = $"-c \"{cmd} {args}\""
            };

            Process process = new Process { StartInfo = psi };
            process.Start();

            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (error != null) {
                return BadRequest();
            }

            if (!await AmazonS3Operations.BucketExists(recordingInfo["user_name"])) {
                await AmazonS3Operations.CreateBucket(recordingInfo["user_name"]);
            }

            await AmazonS3Operations.AddRecordingToBucket(recordingInfo["user_name"], Path.GetFullPath(selectedVideo["title"] + ".mov"));

            recordingInfo["key"] = selectedVideo["title"] + ".mov";
            recordingInfo["filePath"] = Path.GetFullPath(selectedVideo["title"] + ".mov"); 
            OkObjectResult res2 = new OkObjectResult(recordingInfo);

            await _kafkaProducer.ProduceMessageAsync("ProcessGameRecording", JsonConvert.SerializeObject(recordingInfo), recordingInfo["gameId"]);

            return Ok(res2);
        }
        catch {
            return BadRequest();
        }
    }

    [HttpPost("/ProcessRecordingMQ")]
    public async Task<ActionResult> AddToProcessRecordQueue(Dictionary<string, object> reqBody) {
        SingleGame? game = await _singleGameService.GetGame(reqBody["gameId"].ToString()!);
        if (game == null) {
            return NotFound();
        }
        _producer.SendMessage("ProcessRecord", reqBody);
        return Ok();
    }

    [HttpPost("/downloadVideo")]
    public async Task<ActionResult> DownloadVideo([FromBody] Dictionary<String, String> downloadInfo) {
        try {
            var res = await AmazonS3Operations.DownloadVideoFromBucket(downloadInfo["bucketName"], downloadInfo["key"], downloadInfo["filePath"]);
            if (!res) {
                return BadRequest();
            }
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("/AddFilePath/{gameId}/{videoPath}")]
    public async Task<ActionResult> AddVideoFilePath(string gameId, string videoPath) {
        try {
            var user = await client.GetAsync("/SingleGame/" + gameId);
            if ((int) user.StatusCode != 200) {
                return NotFound();
            }

            await _singleGameService.AddVideoFilePath(videoPath, gameId);
            return Ok();
        } catch {
            return BadRequest();
        }
    }
}
