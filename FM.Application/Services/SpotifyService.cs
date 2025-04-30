using FM.Application.Services.ServiceDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FM.Application.Services
{
    public class SpotifyService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SpotifyService> _logger;

        public SpotifyService(HttpClient httpClient, ILogger<SpotifyService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri("https://api.spotify.com/v1/");
                _logger.LogInformation("Fallback BaseAddress applied: https://api.spotify.com/v1/");
            }

            _logger.LogInformation($"BaseAddress: {_httpClient.BaseAddress}");
        }


        public async Task<SpotifyTokenResponse> ExchangeCodeForTokenAsync(string code, string clientId, string clientSecret, string redirectUri)
        {
            try
            {
                using var httpClient = new HttpClient();

                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "redirect_uri", redirectUri }
                });

                var response = await httpClient.PostAsync("https://accounts.spotify.com/api/token", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Spotify token exchange failed: {StatusCode}, {Content}", response.StatusCode, responseContent);
                    return null;
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<SpotifyTokenResponse>(responseContent, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exchanging code for token");
                return null;
            }
        }




        public async Task<List<SpotifyDataDTO>> GetTopTracksAsync(string accessToken, string timeRange = "short_term", int limit = 25)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            _logger.LogInformation($"Request URI: {_httpClient.BaseAddress}me/top/tracks?time_range={timeRange}&limit={limit}");

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
                new AuthenticationHeaderValue("Bearer", accessToken);

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
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync("me");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<SpotifyTokenResponse> RefreshAccessTokenAsync(string refreshToken, string clientId, string clientSecret)
        {
            try
            {
                using var httpClient = new HttpClient();

                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "refresh_token" },
                    { "refresh_token", refreshToken }
                });

                _logger.LogInformation("Attempting to refresh access token");

                var response = await httpClient.PostAsync("https://accounts.spotify.com/api/token", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Spotify token refresh failed: {StatusCode}, {Content}", response.StatusCode, responseContent);
                    return null;
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<SpotifyTokenResponse>(responseContent, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing access token");
                return null;
            }
        }

    }
}
