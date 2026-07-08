using MusicAiAgent.Models;
using SpotifyAPI.Web.Http;
using System.Text;
using System.Text.Json;


namespace MusicAiAgent.Services;

public class GroqAiService
{
    private readonly HttpClient _httpclient;
    private readonly string _apiKey;

    public GroqAiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpclient = httpClient;
        _apiKey = configuration["AiOptions:ApiKey"];
    }

    public async Task<SpotifyMoodParams> AnalyzeMoodAsync(string userMood, string language)
    {
        //sistem promptu
        var systemPrompt = @"Sen bir müzik asistanısın. Kullanıcının ruh haline ve dil tercihine göre Spotify'da arama yapmak için 2-3 kelimelik, nokta atışı bir İNGİLİZCE arama terimi (Search Query) üretmelisin. 

        Kullanıcı Türkçe müzik istiyorsa arama teriminin sonuna 'turkish' kelimesini ekle (Örn: 'turkish pop', 'turkish rap', 'turkish rock', 'turkish slow').
        Kullanıcı yabancı müzik istiyorsa tamamen genel İngilizce terimler üret (Örn: 'high energy workout', 'sad acoustic chill', 'focus lo-fi', 'synthwave gaming').

        SADECE JSON formatında yanıt ver: { ""searchQuery"": ""ürettiğin_ingilizce_terim"" }. Ekstra hiçbir açıklama yapma.";

        var requestBody = new
        {
            model = "llama-3.1-8b-instant",
            response_format = new { type = "json_object" },
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = $"Kullanıcının ruh hali: '{userMood}, istediği dil: {language}" }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        // groq api istek atma
        _httpclient.DefaultRequestHeaders.Clear();
        _httpclient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        var response = await _httpclient.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);

        // eğer istek başarısız olursa, Groq'un asıl hata mesajını okuyup ekrana bas
        if (!response.IsSuccessStatusCode)
        {
            var errorDetail = await response.Content.ReadAsStringAsync();
            throw new Exception($"Groq API hatası, Sebep: {errorDetail}");
        }

        var responseString = await response.Content.ReadAsStringAsync();

        // groq yanıtındaki jsondan llamanın metnini çıkartma

        using var jsonDocument = JsonDocument.Parse(responseString);
        var aiContent = jsonDocument.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        // metning c# modeline dönüştürülmesi
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true, // büyük/küçük harf duyarlılığını kaldır
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString // ayı tırnak içinde gelse bile double'a çevir
        };

        var moodParams = JsonSerializer.Deserialize<SpotifyMoodParams>(aiContent, options);
        return moodParams;
    }
}
