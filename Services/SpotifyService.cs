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

    // liste dinamikleştirildi
    public async Task<List<SpotifyTrackDto>> GetRecommendationsAsync(string searchQuery, int limit, int offset)
    {
        var config = SpotifyClientConfig.CreateDefault();
        var request = new ClientCredentialsRequest(_clientId, _clientSecret);
        var authResponse = await new OAuthClient(config).RequestToken(request);
        var spotify = new SpotifyClient(config.WithToken(authResponse.AccessToken));

        var searchRequest = new SearchRequest(SearchRequest.Types.Track, searchQuery)
        {
            Limit = limit,
            Market = "TR",
            Offset = offset
        };

        var searchResponse = await spotify.Search.Item(searchRequest);
        var trackList = new List<SpotifyTrackDto>();

        if (searchResponse.Tracks.Items != null)
        {
            var uniqueTracks = searchResponse.Tracks.Items
                .DistinctBy(t => t.Name)
                .Take(limit) 
                .ToList();

            foreach (var track in uniqueTracks)
            {
                trackList.Add(new SpotifyTrackDto
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