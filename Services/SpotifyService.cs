using SpotifyAPI.Web;
using MusicAiAgent.Models;

namespace MusicAiAgent.Services;

public class SpotifyService
{
    private readonly string _clientId;
    private readonly string _clientSecret;

    public SpotifyService(IConfiguration config)
    {
        _clientId = config["SpotifyOptions:ClientId"];
        _clientSecret = config["SpotifyOptions:ClientSecret"];
    }

    public async Task<List<object>> GetRecommendationsAsync(SpotifyMoodParams aiParams)
    {
        // 1. yetkilendirme
        var config = SpotifyClientConfig.CreateDefault();
        var request = new ClientCredentialsRequest(_clientId, _clientSecret);
        var authResponse = await new OAuthClient(config).RequestToken(request);
        var spotify = new SpotifyClient(config.WithToken(authResponse.AccessToken));

        Random rnd = new Random();
        int rastgeleOfset = rnd.Next(0, 5) * 10;

        // 2. yapay zekadan gelen arama terimiyle arama yapıyorum
        var searchRequest = new SearchRequest(SearchRequest.Types.Track, aiParams.SearchQuery);
        searchRequest.Limit = 10;
        searchRequest.Market = "TR";
        searchRequest.Offset = rastgeleOfset;

        var searchResponse = await spotify.Search.Item(searchRequest);
        var trackList = new List<object>();

        if (searchResponse.Tracks.Items != null)
        {
            // 3. aynı isimdeki şarkıları eliyorum ve en tepedeki 10 tanesini alıyorum.
            var uniqueTracks = searchResponse.Tracks.Items
                .DistinctBy(t => t.Name)
                .Take(10)
                .ToList();

            foreach (var track in uniqueTracks)
            {
                trackList.Add(new
                {
                    SarkiAdi = track.Name,
                    Sanatci = track.Artists.FirstOrDefault()?.Name,
                    KapakFotografi = track.Album.Images.FirstOrDefault()?.Url,
                    SpotifyLinki = track.ExternalUrls.ContainsKey("spotify") ? track.ExternalUrls["spotify"] : null,
                    OnizlemeSesi = track.PreviewUrl,
                    AlbumAdi = track.Album.Name,
                    YayinTarihi = track.Album.ReleaseDate,
                    Populerlik = track.Popularity,
                    Uri = track.Uri
                });
            }
        }

        return trackList;
    }
}