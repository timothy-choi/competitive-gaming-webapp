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
using Microsoft.AspNetCore.Http.HttpResults;

[ApiController]
[Route("[controller]")]
public class WebhookCashApp : Controller {
    [HttpPost]
    public async Task<ActionResult<Dictionary<string, string>>> HandleWebhook() {
        try {
            Dictionary<string, string> resBody = new Dictionary<string, string>();
            using (var res = new StreamReader(Request.Body)) {
                var requestBody = await res.ReadToEndAsync();

                Dictionary<string, dynamic> webhookRes = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(requestBody)!;

                if (webhookRes["status"].ToString() != "Active") {
                    return BadRequest();
                }

                switch (webhookRes["type"].ToString()) {
                    case "customer_request.state.updated":
                        if (webhookRes["data"]["object"]["request"]["status"] == "Declined") {
                            resBody["type"] = webhookRes["type"].ToString();
                            resBody["status"] = "Declined"; 
                        }
                        if (webhookRes["data"]["object"]["request"]["status"] == "Accepted") {
                            resBody["type"] = webhookRes["type"].ToString();
                            resBody["status"] = "Accepted";
                        }
                        break;

                    case "grant.created":
                        if (webhookRes["data"]["object"]["status"] != "ACTIVE") {
                            resBody["type"] = webhookRes["type"].ToString();
                            resBody["status"] = "Invalid";
                        }
                        else {
                            resBody["type"] = webhookRes["type"].ToString();
                            resBody["status"] = "Valid";
                            resBody["grant_id"] = webhookRes["data"]["object"]["grant"]["id"];
                        }
                        break;

                    case "payment.status.updated":
                        if (webhookRes["data"]["object"]["payment"]["status"] != "Authorized" && webhookRes["data"]["object"]["payment"]["status"] != "Captured") {
                            resBody["type"] = webhookRes["type"].ToString();
                            resBody["status"] = "Payment_Error";
                        }
                        else {
                            resBody["type"] = webhookRes["type"].ToString();
                            resBody["status"] = webhookRes["data"]["object"]["payment"]["status"];
                        }
                        break;

                    case "merchant.status.updated":
                        resBody["type"] = webhookRes["type"].ToString();
                        resBody["status"] = webhookRes["data"]["object"]["payment"]["status"];
                        break;

                    default:
                        return BadRequest();
                }
            }
            OkObjectResult updateResponse = new OkObjectResult(resBody);
            return Ok(updateResponse);
        } catch {
            return BadRequest();
        }
    }
}

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
                idempotencyKey = Guid.NewGuid().ToString(),
                webhookEndpoints = new Dictionary<string, string>()
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

    [HttpPost("{username}/Webhooks")]
    public async Task<ActionResult> AddWebhook(string username, Dictionary<string, string> reqBody) {
        try {
            var acct = await _playerPaymentService.PlayerPaymentAccounts.AsQueryable().Where(user => user.playerUsername == username).ToListAsync();
            if (acct == null) {
                return NotFound();
            }

            Dictionary<string, object> webhookContent = new Dictionary<string, object>();

            webhookContent["idempotency_key"] = reqBody["idempotency_key"];
            webhookContent["webhook_endpoint"] = new {
                api_key_id = reqBody["api_key_id"],
                event_configurations = new[] {
                    new
                    {
                        event_type = webhookContent["event_type"]
                    }
                },
                api_version = "v1",
                url = reqBody["url"]
            };

            var content = JsonConvert.SerializeObject(webhookContent);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.cash.app/management/v1/webhook-endpoints"),
                Headers =
                {
                    { "X-Region", "" },
                    { "X-Signature", "" },
                    { "Accept", "application/json" },
                },
                Content = new StringContent(content)
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };

            using (var response = await _client.SendAsync(request)) {
                response.EnsureSuccessStatusCode();

                acct[0].webhookEndpoints[reqBody["event_type"]] = reqBody["url"];

                await _playerPaymentService.SaveChangesAsync();

                return Ok();
            }

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
                var grant_info = JsonConvert.DeserializeObject<Dictionary<string, object>>(body)!;

                Dictionary<string, string> resBody = new Dictionary<string, string>();

                var allGrants = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(resBody["grants"])!;

                if ((string) grant_info["status"] == "approved") {
                    resBody["username"] = username;
                    resBody["grant_id"] = allGrants[0]["id"];
                    resBody["failedStatus"] = false.ToString();
                    resBody["pending"] = false.ToString();
                }
                if ((string) grant_info["status"] == "declined") {
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

    [HttpPost("{username}/Payment")]
    public async Task<ActionResult<Dictionary<string, string>>> ProcessPayment(string username, Dictionary<string, string> reqBody) {
        try {
            var acct = await _playerPaymentService.PlayerPaymentAccounts.AsQueryable().Where(user => user.playerUsername == username).ToListAsync();
            if (acct == null) {
                return NotFound();
            }

            Dictionary<string, object> paymentInfo = new Dictionary<string, object>();

            paymentInfo["idempotency_key"] = reqBody["idempotency_key"];
            paymentInfo["payment"] = new {
                amount = Convert.ToInt32(reqBody["amount"]),
                currency = reqBody["currency"],
                merchant_id = acct[0].MerchantId,
                grant_id = reqBody["grant_id"]
            };

            string content = JsonConvert.SerializeObject(paymentInfo);

            var request = new HttpRequestMessage {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.cash.app/management/v1/webhook-endpoints"),
                Headers =
                {
                    { "X-Region", "" },
                    { "X-Signature", "" },
                    { "Accept", "application/json" },
                },
                Content = new StringContent(content)
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };

            using (var response = await _client.SendAsync(request)) {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var payment_info = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(body)!;
                if (payment_info["payment"]["status"].ToString() == "Voided" || payment_info["payment"]["status"].ToString() == "Declined") {
                    return BadRequest();
                }
                Dictionary<string, string> resBody = new Dictionary<string, string>();
                resBody["username"] = reqBody["username"];
                resBody["payment_id"] = payment_info["payment"]["id"];
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