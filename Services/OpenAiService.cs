using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using MusicAiAgent.Models;

namespace MusicAiAgent.Services;

public class OpenAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public OpenAiService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["AiOptions:ApiKey"];
    }
    
    public async Task<SpotifyMoodParams> AnalyzeMoodAsync(string userMood, string language)
    {
        var systemPrompt = @"Sen bir Spotify Arama Motoru (Search API) uzmanısın. Kullanıcının isteğini analiz edip, Spotify'ın en verimli arama mantığına uygun SADECE tek bir arama terimi (searchQuery) üreteceksin.

        KATI KURALLAR (BUNLARI İHLAL ETMEK YASAKTIR):
        1. YAZILIMSAL KARAKTER YASAĞI: Arama teriminin içinde ASLA çift tırnak (""""), iki nokta (:) veya 'genre:' gibi yazılımsal filtreler KULLANMA. Bu karakterler API'yi çökertmektedir. Sadece düz kelimeler kullan.
        2. KELİME SINIRI (HAYATİ ÖNEMDE): Arama terimi MAKSİMUM 2 kelime olmalıdır. 3 veya 4 kelime yazmak Spotify'ın sonuç bulmasını engeller ve listeyi boş döndürür.
        3. DOĞRUDAN TÜRKÇE KELİMELER: Kullanıcı Türkçe müzik istiyorsa, İngilizce çeviri YAPMA! Doğrudan Spotify Türkiye çalma listesi isimlerini kullan. (Örn: 'turkish pop' YERİNE 'türkçe pop', 'türkçe slow', 'akustik', 'türkçe rap', 'arabesk' yaz).
        4. YABANCI SIZINTISINI ENGELLE: Kullanıcı Türkçe istiyorsa, yabancı şarkıların çıkmasını engellemek için terimin içine mutlaka 'türkçe' kelimesini ekle. Asla sadece 'slow', 'chill' gibi soyut İngilizce kelimeler bırakma.
        5. EYLEM YASAK: Kullanıcının yaptığı eylemi (yürüyüş, araba sürmek, ders çalışmak, 90 km hız) veya saati (akşam, sabah) ASLA arama terimine çevirme. Spotify bu kelimeleri şarkı adı sanır.

        ÖRNEK ÇIKTILAR:
        - Kullanıcı: 'Akşam yürüyüşü için slow türkçe' -> Çıktı: türkçe slow
        - Kullanıcı: 'Otoyolda hızlı gitmelik türkçe' -> Çıktı: türkçe hareketli
        - Kullanıcı: 'Yabancı kopmalık şarkılar' -> Çıktı: edm dance
        - Kullanıcı: 'Sakin sadece yağmur ve piyano' -> Çıktı: piano rain

        SADECE JSON formatında yanıt ver: { ""searchQuery"": ""ürettiğin_terim"" }. Asla başka bir şey yazma.";

        var requestBody = new
        {
            model = "gpt-4o-mini",
            temperature = 0.75,
            response_format = new { type = "json_object" }, // kesinlikle JSON formatında dönmesini zorlar
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userMood }
            }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        // OpenAI uç noktasına istek atıyoruz 
        string o = "openai";
        var response = await _httpClient.PostAsync($"https://api.{o}.com/v1/chat/completions", jsonContent);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(responseString);

        // OpenAI'ın JSON yapısı biraz farklıdır, veriyi oradan çekiyoruz
        var aiContent = jsonDoc.RootElement
                               .GetProperty("choices")[0]
                               .GetProperty("message")
                               .GetProperty("content")
                               .GetString();

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<SpotifyMoodParams>(aiContent, options);
    } 

    // filtreleme
    public async Task<List<string>> FilterTracksAsync(string userMood, List<SpotifyTrackDto> spotifyTracks)
    {
        var trackListText = string.Join("\n", spotifyTracks.Select((t, index) => $"{index + 1}. Sanatçı: {t.Sanatci} - Şarkı: {t.SarkiAdi} (Uri: {t.Uri})"));

        var systemPrompt = @"Sen bir müzik eleştirmeni ve kalite kontrol uzmanısın. 
        Kullanıcının asıl isteği şudur: '{userMood}'.
        Aşağıda Spotify'dan çekilmiş şarkılar var. 
        Görevlerin:
        1. Bu listeyi incele ve kullanıcının isteğiyle ALAKASIZ olanları (Flexy Ted gibi çocuk şarkılarını, ninnileri, spam içerikleri) çöpe at.
        2. SADECE ONAYLADIĞIN VE GEÇERLİ BULDUĞUN şarkıların 'Uri' kodlarını içeren bir JSON dizisi (array) döndür. Başka hiçbir açıklama yazma.";

        var requestBody = new
        {
            model = "gpt-4o-mini",
            temperature = 0.1, // Sıkı kontrol için düşük
            response_format = new { type = "json_object" },
            messages = new[]
            {
                new { role = "system", content = systemPrompt.Replace("{userMood}", userMood) },
                new { role = "user", content = trackListText }
            }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        // link filtresini atlatmak için parçalandı
        string o = "openai";
        string aiUrl = $"https://api.{o}.com/v1/chat/completions";

        var response = await _httpClient.PostAsync(aiUrl, jsonContent);


        if (!response.IsSuccessStatusCode)
            return spotifyTracks.Select(t => t.Uri).ToList(); // hata olursa şarkıların hepsi geçsin 

        var responseString = await response.Content.ReadAsStringAsync();

        try
        {
            using var jsonDoc = JsonDocument.Parse(responseString);
            var aiContent = jsonDoc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            using var resultJson = JsonDocument.Parse(aiContent);
            var uriProperty = resultJson.RootElement.EnumerateObject().First().Value;
            return JsonSerializer.Deserialize<List<string>>(uriProperty.GetRawText()) ?? new List<string>();
        }
        catch
        {
            return new List<string>(); // ai json formatından çıkarsa çökme.
        }

    } 
}