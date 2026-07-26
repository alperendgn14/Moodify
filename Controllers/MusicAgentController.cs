using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MusicAiAgent.Models;
using MusicAiAgent.Services;
using SpotifyAI.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MusicAiAgent.Models;

namespace MusicAiAgent.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("MoodifyKalkani")]
public class MusicAgentController : ControllerBase
{
    private readonly OpenAiService _aiService;
    private readonly OpenAiService _openAiService;
    private readonly SpotifyService _spotifyService;

    public MusicAgentController(OpenAiService openAiService, SpotifyService spotifyService)
    {
        _openAiService = openAiService;
        _spotifyService = spotifyService;
    }

    [HttpPost("recommend")]
    public async Task<IActionResult> GetMusicRecommendation([FromBody] UserMoodRequest request)
    {
        try
        {
            // arama terimini al
            var aiParams = await _openAiService.AnalyzeMoodAsync(request.UserMood, request.Language);

            int targetCount = 10; // bize tam olarak 10 şarkı lazım
            int currentOffset = new Random().Next(0, 3) * 10; 

            var finalTracks = new List<SpotifyTrackDto>();
            int maxRetries = 4; 
            int retryCount = 0;
           
            while (finalTracks.Count < targetCount && retryCount < maxRetries)
            {
                
                int needed = targetCount - finalTracks.Count;

             
                var fetchedTracks = await _spotifyService.GetRecommendationsAsync(aiParams.SearchQuery, needed, currentOffset);

                if (fetchedTracks.Count == 0) break; 

                // yapay zekaya uygun mu diye şarkılar gönderiliyor
                var validUris = await _openAiService.FilterTracksAsync(request.UserMood, fetchedTracks);

                // geçerli şarkıları listeye ekle   
                var validTracks = fetchedTracks.Where(t => validUris.Contains(t.Uri)).ToList();
                finalTracks.AddRange(validTracks);

                currentOffset += needed;
                retryCount++;
            }

            return Ok(new
            {
                AramaTerimi = aiParams.SearchQuery,
                Sarkilar = finalTracks
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Bir hata oluştu: {ex.Message}");
        }
    }

    [HttpPost("create-playlist")]
    public async Task<IActionResult> CreatePlaylist([FromHeader(Name = "Authorization")] string token, [FromBody] PlaylistCreationRequest request)
    {
        if (string.IsNullOrEmpty(token)) return Unauthorized();

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));

       
        string s = "spotify";
        string api = $"https://api.{s}.com/v1";

        // 1-playlist oluştur
        var playlistBody = new
        {
            name = request.PlaylistName,
            description = "Moodify AI tarafından ruh halinize göre üretilmiştir.",
            @public = false
        };
        var playlistContent = new StringContent(JsonSerializer.Serialize(playlistBody), Encoding.UTF8, "application/json");

        
        var playlistResponse = await client.PostAsync($"{api}/me/playlists", playlistContent);
        if (!playlistResponse.IsSuccessStatusCode)
        {
            var errorBody = await playlistResponse.Content.ReadAsStringAsync();
            return BadRequest($"Liste oluşturulamadı. Hata: {errorBody}");
        }

        var playlistJson = JsonDocument.Parse(await playlistResponse.Content.ReadAsStringAsync());
        var playlistId = playlistJson.RootElement.GetProperty("id").GetString();
        var playlistUrl = playlistJson.RootElement.GetProperty("external_urls").GetProperty("spotify").GetString();

        // 2-şarkı ekle
        var tracksBody = new
        {
            uris = request.TrackUris,
            position = 0
        };
        var tracksContent = new StringContent(JsonSerializer.Serialize(tracksBody), Encoding.UTF8, "application/json");

        var tracksResponse = await client.PostAsync($"{api}/playlists/{playlistId}/items", tracksContent);
        if (!tracksResponse.IsSuccessStatusCode)
        {
            var errorBody = await tracksResponse.Content.ReadAsStringAsync();
            return BadRequest($"Şarkılar eklenemedi. Hata: {errorBody}");
        }

        return Ok(new { playlistUrl });
    }
}

public class PlaylistCreationRequest
    {
        public string PlaylistName { get; set; }
        public List<string> TrackUris { get; set; }
    }


