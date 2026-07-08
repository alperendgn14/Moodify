using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MusicAiAgent.Models;
using MusicAiAgent.Services;
using SpotifyAI.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MusicAiAgent.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("MoodifyKalkani")]
public class MusicAgentController : ControllerBase
{
    private readonly OpenAiService _aiService;
    private readonly SpotifyService _spotifyService;

    public MusicAgentController(OpenAiService aiService, SpotifyService spotifyService)
    {
        _aiService = aiService;
        _spotifyService = spotifyService;
    }

    [HttpPost("recommend")]
    public async Task<IActionResult> GetMusicRecommendations([FromBody] MoodRequestDto request)
    {
        if (string.IsNullOrEmpty(request.UserMood))
            return BadRequest("Ruh hali boş bırakılamaz.");

        try
        {
            // 1. ruh halini llamaya gönder
            var spotifyParams = await _aiService.AnalyzeMoodAsync(request.UserMood, request.Language);

            // 2. çıkan değerleri spotify'a yolla ve şarkıları getir.
            var recommendedSongs = await _spotifyService.GetRecommendationsAsync(spotifyParams);

            // 3. sonucu frontende yolla.
            return Ok(new
            {
                Mesaj = "İşte ruh haline tam uyan şarkılar!",
                YapayZekaAnalizi = spotifyParams,
                Sarkilar = recommendedSongs
            });
        }
        catch (SpotifyAPI.Web.APIException ex)
        {
            // Spotify'ın gizli JSON hatasını zorla metne çevirip okuyoruz
            var errorJson = System.Text.Json.JsonSerializer.Serialize(ex.Response?.Body);
            return StatusCode(500, $"Spotify'ın Gizli Hatası:\nKod: {ex.Response?.StatusCode}\nDetay: {errorJson}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"ÇÖKME DETAYI:\nMesaj: {ex.Message}\nİz: {ex.StackTrace}");
        }
    }

    [HttpPost("create-playlist")]
    public async Task<IActionResult> CreatePlaylist([FromHeader(Name = "Authorization")] string token, [FromBody] PlaylistCreationRequest request)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized();

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("Bearer", ""));

        //1-kullanıcının spotify id öğren
        var userResponse = await client.GetAsync("https://api.spotify.com/v1/me");
        if (!userResponse.IsSuccessStatusCode)
            return BadRequest("Kullanıcı profili alınamadı.");

        var userJson = JsonDocument.Parse(await userResponse.Content.ReadAsStringAsync());
        var userId = userJson.RootElement.GetProperty("id").GetString();


        //2-kullanıcı adıyla playlist oluştur.
        var playlistBody = new { name = request.PlaylistName, description = "Moodify AI tarafından ruh halinize göre üretilmiştir."};
        var playlistContent = new StringContent(JsonSerializer.Serialize(playlistBody), Encoding.UTF8, "application/json");

        var playlistResponse = await client.PostAsync($"https://api.spotify.com/v1/users/{userId}/playlists", playlistContent);
        if (!playlistResponse.IsSuccessStatusCode)
            return BadRequest("Çalma listesi oluşturulamadı.");

        var playlistJson = JsonDocument.Parse(await playlistResponse.Content.ReadAsStringAsync());
        var playlistId = playlistJson.RootElement.GetProperty("id").GetString();
        var playlistUrl = playlistJson.RootElement.GetProperty("external_urls").GetProperty("spotify").GetString();

        //3-şarkılar playliste eklenir
        var tracksBody = new { uris = request.TrackUris };
        var tracksContent = new StringContent(JsonSerializer.Serialize(tracksBody), Encoding.UTF8, "application/json");

        var tracksResponse = await client.PostAsync($"https://api.spotify.com/v1/playlists/{playlistId}/tracks", tracksContent);
        if (!tracksResponse.IsSuccessStatusCode)
            return BadRequest("Şarkılar listeye eklenemedi.");

        return Ok(new { playlistUrl });
    }

    public class PlaylistCreationRequest
    {
        public string PlaylistName { get; set; }
        public List<string> TrackUris { get; set; }
    }


}