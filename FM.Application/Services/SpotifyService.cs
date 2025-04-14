using FM.Application.Services.ServiceDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FM.Application.Services
{
    public class SpotifyService
    {
        private readonly HttpClient _httpClient;

        public SpotifyService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri("https://api.spotify.com/v1/");
                Console.WriteLine("Fallback BaseAddress applied: https://api.spotify.com/v1/");
            }

            Console.WriteLine($"BaseAddress: {_httpClient.BaseAddress}");
        }


        public async Task<List<SpotifyDataDTO>> GetTopTracksAsync(string accessToken, string timeRange = "short_term", int limit = 25)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            Console.WriteLine($"Request URI: {_httpClient.BaseAddress}me/top/tracks?time_range={timeRange}&limit={limit}");

            var response = await _httpClient.GetAsync($"me/top/tracks?time_range={timeRange}&limit={limit}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            var json = JsonDocument.Parse(content);
            var items = json.RootElement.GetProperty("items");

            return items.EnumerateArray().Select(item =>
            {
                var album = item.GetProperty("album");
                var artists = item.GetProperty("artists");

                return new SpotifyDataDTO
                {
                    SongName = item.GetProperty("name").GetString(),
                    Artist = string.Join(", ", artists.EnumerateArray().Select(a => a.GetProperty("name").GetString())),
                    Album = album.GetProperty("name").GetString(),
                    AlbumImageUrl = album.GetProperty("images")[0].GetProperty("url").GetString(),
                    SongUrl = item.GetProperty("external_urls").GetProperty("spotify").GetString(),
                    Duration = TimeSpan.FromMilliseconds(item.GetProperty("duration_ms").GetInt32())
                };

            }).ToList();
        }

        public async Task<SpotifyDataDTO?> GetCurrentlyPlayingTrackAsync(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync("me/player/currently-playing");
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return null; // No song is currently playing
            }

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            // Parse the JSON response
            var json = JsonDocument.Parse(content);
            var root = json.RootElement;

            var item = root.GetProperty("item");
            var album = item.GetProperty("album");
            var artists = item.GetProperty("artists");

            return new SpotifyDataDTO
            {
                SongName = item.GetProperty("name").GetString(),
                Artist = string.Join(", ", artists.EnumerateArray().Select(a => a.GetProperty("name").GetString())),
                Album = album.GetProperty("name").GetString(),
                AlbumImageUrl = album.GetProperty("images")[0].GetProperty("url").GetString(),
                SongUrl = item.GetProperty("external_urls").GetProperty("spotify").GetString(),
                Duration = TimeSpan.FromMilliseconds(item.GetProperty("duration_ms").GetInt32()),
                Progress = TimeSpan.FromMilliseconds(root.GetProperty("progress_ms").GetInt32()),
                isPlaying = root.GetProperty("is_playing").GetBoolean()
            };
        }





        public async Task<string> GetUserProfileAsync(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync("me"); // Use relative URI
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
