namespace CompetitiveGamingApp.Controllers;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;

[ApiController]
[Route("api/playerPayment")]
public class PlayerPaymentController : ControllerBase {
    private readonly PlayerPaymentServices _playerPaymentService;
    private readonly HttpClient _client;
    public PlayerPaymentController(PlayerPaymentServices playerPaymentServices) {
        _playerPaymentService = playerPaymentServices;
        _client = new HttpClient();
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<PlayerPaymentAccount>> getPaymentAccount(string username) {
        try {
            var player = await _playerPaymentService.PlayerPaymentAccounts.AsQueryable().Where(user => user.playerUsername == username).ToListAsync();
            if (player == null) {
                return BadRequest();
            }
            OkObjectResult res = new OkObjectResult(player[0]);
            return Ok(res);
        } catch {
            return BadRequest();
        }
    }

    [HttpPost]
    public async Task<ActionResult> createNewAccount([FromBody] Dictionary<string, string> acctInfo) {
        try {
            PlayerPaymentAccount playerAcct = new PlayerPaymentAccount {
                playerPaymentAccountId = Guid.NewGuid().ToString(),
                playerUsername = acctInfo["username"],
                playerPaymentUsername = acctInfo["cashAppUsername"],
                playerCashAppId = acctInfo["playerCashAppId"],
                MerchantId = acctInfo["MerchantId"],
                idempotencyKey = Guid.NewGuid().ToString()
            };

            await _playerPaymentService.AddAsync(playerAcct);
            await _playerPaymentService.SaveChangesAsync();
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("{username}/Merchant")]
    public async Task<ActionResult<Dictionary<string, string>>> createMerchantAccount(string username, Dictionary<string, string> reqBody) {
        try {
            var content = JsonConvert.SerializeObject(reqBody);
            var request = new HttpRequestMessage {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.cash.app/network/v1/merchants"),
                Headers = {
                    { "X-Region", "" },
                    { "X-Signature", "" },
                    { "Accept", "application/json" },
                },
                Content = new StringContent(content) {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };

            using (var response = await _client.SendAsync(request)) {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var merchant = JsonConvert.DeserializeObject<Dictionary<string, string>>(body)!;
                Dictionary<string, string> resBody = new Dictionary<string, string>();

                resBody["username"] = username;
                resBody["merchant_id"] = merchant["id"];
                OkObjectResult merchantInfo = new OkObjectResult(resBody);

                return Ok(merchantInfo);
            }
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("{username}/{merchantId}/Merchants")]
    public async Task<ActionResult> UpdateMerchantId(string username, string merchantId) {
        try {
            var acct = await _playerPaymentService.PlayerPaymentAccounts.AsQueryable().Where(user => user.playerUsername == username).ToListAsync();
            if (acct == null) {
                return NotFound();
            }
            
            acct[0].MerchantId = merchantId;

            await _playerPaymentService.SaveChangesAsync();

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("{username}/Grant")]
    public async Task<ActionResult<Dictionary<string, string>>> RequestCustomerGrant(string username, Dictionary<string, string> reqBody) {
        try {
            var content = JsonConvert.SerializeObject(reqBody);
            var request = new HttpRequestMessage {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.cash.app/customer-request/v1/requests"),
                Headers =
                {
                    { "Accept", "application/json" },
                },
                Content = new StringContent(content) {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };

            using (var response = await _client.SendAsync(request)) {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var grant_info = JsonConvert.DeserializeObject<Dictionary<string, string>>(body)!;

                Dictionary<string, string> resBody = new Dictionary<string, string>();

                var allGrants = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(resBody["grants"])!;

                if (grant_info["status"] == "approved") {
                    resBody["username"] = username;
                    resBody["grant_id"] = allGrants[0]["id"];
                    resBody["failedStatus"] = false.ToString();
                    resBody["pending"] = false.ToString();
                }
                if (grant_info["status"] == "declined") {
                    resBody["username"] = username;
                    resBody["failedStatus"] = true.ToString();
                    resBody["pending"] = false.ToString();
                }
                else {
                    resBody["username"] = username;
                    resBody["failedStatus"] = false.ToString();
                    resBody["pending"] = true.ToString();
                }

                OkObjectResult res = new OkObjectResult(resBody);
                return Ok(res);
            }
        } catch {
            return BadRequest();
        }
    }

    [HttpPut("{username}/IdempotencyKey")]
    public async Task<ActionResult> changeIdempotencyKey(string username) {
        try {
            var acct = await _playerPaymentService.PlayerPaymentAccounts.AsQueryable().Where(user => user.playerUsername == username).ToListAsync();
            if (acct == null) {
                return NotFound();
            }
            
            byte[] randomNumber = new byte[16]; // 16 bytes for a GUID

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomNumber);
            }

            Guid randomGuid = new Guid(randomNumber);

            acct[0].idempotencyKey = randomGuid.ToString();

            await _playerPaymentService.SaveChangesAsync();

            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpDelete("{username}")]
    public async Task<ActionResult> DeleteAccount(string username) {
        try {
            var acct = await _playerPaymentService.PlayerPaymentAccounts.AsQueryable().Where(user => user.playerUsername == username).ToListAsync();
            if (acct == null) {
                return NotFound();
            }
            
            _playerPaymentService.Remove(acct);
            await _playerPaymentService.SaveChangesAsync();
            return Ok();
        } catch {
            return BadRequest();
        }
    }
}