namespace CompetitiveGamingApp.Controllers;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using System.Text;

[ApiController]
[Route("api/playerAuth")]

public class PlayerAuthController : ControllerBase {
    private readonly PlayerAuthServices _playerAuthServices;
    private readonly IConfiguration _configuration;

    public PlayerAuthController(IConfiguration configuration, PlayerAuthServices playerAuthServices) {
        _playerAuthServices = playerAuthServices;
        _configuration = configuration;
    }

    private static string HashPassword(string password) {
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            RandomNumberGenerator.GetBytes(64),
            400000,
            HashAlgorithmName.SHA512,
            64
        );

        return Convert.ToHexString(hash);
    }

    private static bool CheckPassword(string password, string hashedPassword) {
        var comparedHash = Rfc2898DeriveBytes.Pbkdf2(password, RandomNumberGenerator.GetBytes(64), 400000, HashAlgorithmName.SHA512, 64);
        return CryptographicOperations.FixedTimeEquals(comparedHash, Convert.FromHexString(hashedPassword));
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] Dictionary<string, string> authInfo) {
        try {
            if (HttpContext.Session.Keys.Contains("username")) {
                return Unauthorized();
            }
            var acct = await _playerAuthServices.playerAuths.AsQueryable().Where(u => u.PlayerUsername == authInfo["username"]).ToListAsync();
            if (acct != null) {
                return BadRequest();
            }
            if (authInfo["password"].Length < 9 || !authInfo["password"].Any(Char.IsDigit) || !authInfo["password"].Any(c => !Char.IsLetterOrDigit(c)) || !authInfo["password"].Any(char.IsLetter) || !authInfo["password"].Equals(authInfo["retype_password"])) {
                return BadRequest();
            }

            PlayerAuth currAuth = new PlayerAuth {
                PlayerAuthId = Guid.NewGuid().ToString(),
                PlayerUsername = authInfo["username"],
                PlayerPassword = HashPassword(authInfo["password"])
            };

            await _playerAuthServices.AddAsync(currAuth);
            await _playerAuthServices.SaveChangesAsync();
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] Dictionary<string, string> loginInfo) {
        try {
            var acct = await _playerAuthServices.playerAuths.AsQueryable().Where(u => u.PlayerUsername == loginInfo["username"]).ToListAsync();
            if (acct == null) {
                Console.WriteLine(1);
                return BadRequest();
            }

            var hashedPassword = HashPassword(loginInfo["password"]);
            if (!CheckPassword(acct[0].PlayerPassword!, hashedPassword)) {
                Console.WriteLine(2);
                return BadRequest();
            }
            HttpContext.Session.SetString("username", loginInfo["username"]);
            _configuration["WebSocketConfig:StopServer"] = "Start";
            return Ok();
        } 
        catch (Exception e) {
            Console.WriteLine(e.ToString());
            return BadRequest();
        }
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout() {
        if (HttpContext.Session.Keys.Contains("username")) {
            return Unauthorized();
        }
        HttpContext.Session.Remove("username");
        _configuration["WebSocketConfig:StopServer"] = "Stop";
        return Ok();
    }

    [HttpPut("{username}/{newUsername}")]
    public async Task<ActionResult> ChangeUsername(string username, string newUsername) {
        if (!HttpContext.Session.Keys.Contains("username")) {
            return Unauthorized();
        }
        try {
            var acct = await _playerAuthServices.playerAuths.AsQueryable().Where(u => u.PlayerUsername == username).ToListAsync();
            if (acct == null) {
                return BadRequest();
            }

            var found = await _playerAuthServices.playerAuths.AsQueryable().Where(u => u.PlayerUsername == newUsername).ToListAsync();
            if (found != null) {
                return BadRequest();
            }

            acct[0].PlayerUsername = newUsername;
            await _playerAuthServices.SaveChangesAsync();
            return Ok();
        }
        catch {
            return BadRequest();
        }
    }

    [HttpPut("{username}/{newPassword}/{retypePassword}")]
    public async Task<ActionResult> ChangePassword(string username, string newPassword, string retypePassword) {
        if (!HttpContext.Session.Keys.Contains("username")) {
            return Unauthorized();
        }
        try {
            var acct = await _playerAuthServices.playerAuths.AsQueryable().Where(u => u.PlayerUsername == username).ToListAsync();
            if (acct == null) {
                return BadRequest();
            }

            if (newPassword.Length < 9 || !newPassword.Any(Char.IsDigit) || !newPassword.Any(c => !Char.IsLetterOrDigit(c)) || !newPassword.Any(char.IsLetter) || !newPassword.Equals(retypePassword)) {
                return BadRequest();
            }

            acct[0].PlayerPassword = HashPassword(newPassword);
            await _playerAuthServices.SaveChangesAsync();
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpDelete("{username}")]
    public async Task<ActionResult> DeleteAcct(string username) {
        if (!HttpContext.Session.Keys.Contains("username")) {
            return Unauthorized();
        }
        try {
            var acct = await _playerAuthServices.playerAuths.AsQueryable().Where(u => u.PlayerUsername == username).ToListAsync();
            if (acct == null) {
                return BadRequest();
            }
            _playerAuthServices.Remove(acct);
            await _playerAuthServices.SaveChangesAsync();
            return Ok();
        } catch {
            return BadRequest();
        }
    }

    [HttpGet("session/{username}")]
    public async Task<ActionResult<bool>> GetSession(string username) {
        try {
            if (!HttpContext.Session.Keys.Contains("username")) {
                return Ok("Not Logged In");
            }
            var res = HttpContext.Session.GetString("username") == username ? true : false;
            return Ok(res);
        } catch {
            return BadRequest();
        }
    }
}