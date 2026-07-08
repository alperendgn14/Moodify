using Microsoft.AspNetCore.Mvc;
using MusicAiAgent.Models;
using MusicAiAgent.Services;
using SpotifyAI.Models;

namespace MusicAiAgent.Controllers;

[ApiController]
[Route("api/[controller]")]
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
}