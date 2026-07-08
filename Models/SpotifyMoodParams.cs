using System.Text.Json.Serialization;

namespace MusicAiAgent.Models;

public class SpotifyMoodParams
{
    [JsonPropertyName("searchQuery")]
    public string SearchQuery { get; set; } // Örn: "high energy workout", "sad acoustic chill"
}