using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MusicAiAgent.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var clientId = _config["SpotifyOptions:ClientId"];
        var scheme = Request.Scheme;
        var host = Request.Host;
        //localhost veya render adresi
        var redirectUri = $"{scheme}://{host}/api/auth/callback";

        //çalma listesi oluşturma , ekleme
        var scopes = "playlist-modify-public playlist-modify-private";

        var spotifyAuthUrl = $"https://accounts.spotify.com/authorize?client_id={clientId}&response_type=code&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope={Uri.EscapeDataString(scopes)}";

        return Redirect(spotifyAuthUrl);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code)
    {
        var clientId = _config["SpotifyOptions:ClientId"];
        var clientSecret = _config["SpotifyOptions:ClientSecret"];
        var scheme = Request.Scheme;
        var host = Request.Host;
        var redirectUri = $"{scheme}://{host}/api/auth/callback";

        using var client = new HttpClient();
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");

        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

        var keyValues = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "authorization_code"),
            new("code", code),
            new("redirect_uri", redirectUri)
        };

        requestMessage.Content = new FormUrlEncodedContent(keyValues);

        var response = await client.SendAsync(requestMessage);
        if (!response.IsSuccessStatusCode)
            return BadRequest("Spotify'dan token alınamadı.");

        var responseString = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(responseString);
        var accessToken = jsonDoc.RootElement.GetProperty("access_token").GetString();

        //giriş başarılıysa tokeni frontende aktarma
        return Redirect($"/index.html?access_token={accessToken}");

    }
}
