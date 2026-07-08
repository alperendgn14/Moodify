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
        var redirectUri = $"{Request.Scheme}://{Request.Host}/api/auth/callback";
        var scopes = "playlist-modify-public playlist-modify-private user-read-private";

        // url parçalama
        string spotifyAuth = "https://accounts." + "spotify.com";

        var authorizeUrl = $"{spotifyAuth}/authorize?client_id={clientId}&response_type=code&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope={Uri.EscapeDataString(scopes)}";

        return Redirect(authorizeUrl);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code)
    {
        var clientId = _config["SpotifyOptions:ClientId"];
        var clientSecret = _config["SpotifyOptions:ClientSecret"];
        var redirectUri = $"{Request.Scheme}://{Request.Host}/api/auth/callback";

        using var client = new HttpClient();

        string spotifyAuth = "https://accounts." + "spotify.com";
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{spotifyAuth}/api/token");

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
        if (!response.IsSuccessStatusCode) return BadRequest("Spotify'dan token alınamadı.");

        var responseString = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(responseString);
        var accessToken = jsonDoc.RootElement.GetProperty("access_token").GetString();

        return Redirect($"/index.html?access_token={accessToken}");
    }
}