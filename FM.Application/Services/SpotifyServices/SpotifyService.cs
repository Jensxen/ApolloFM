using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using FM.Application.Services.ServiceDTO.SpotifyDTO;

namespace FM.Application.Services.SpotifyServices
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
                _logger.LogInformation("Starting token exchange with code: {CodeStart}..., redirect URI: {RedirectUri}",
                    code.Substring(0, Math.Min(5, code.Length)), redirectUri);

                using var httpClient = new HttpClient();

                // Set up the Basic Auth header with client_id and client_secret
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                _logger.LogInformation("Added authorization header with credentials");

                // Create the form data for the token request
                var formData = new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "redirect_uri", redirectUri }
                };

                _logger.LogInformation("Form data: grant_type={GrantType}, code={CodeStart}..., redirect_uri={RedirectUri}",
                    "authorization_code", code.Substring(0, Math.Min(5, code.Length)), redirectUri);

                var content = new FormUrlEncodedContent(formData);

                _logger.LogInformation("Sending POST request to Spotify token endpoint");

                // Send the request to Spotify's token endpoint
                var response = await httpClient.PostAsync("https://accounts.spotify.com/api/token", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Received response from Spotify: Status {StatusCode}", response.StatusCode);

                // Log the actual response content in a safe way
                if (responseContent.Length > 100)
                {
                    _logger.LogDebug("Response content: {Content}...", responseContent.Substring(0, 100));
                }
                else
                {
                    _logger.LogDebug("Response content: {Content}", responseContent);
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Spotify token exchange failed: {StatusCode}, {Content}",
                        response.StatusCode, responseContent);
                    return null;
                }

                _logger.LogInformation("Successfully received token response");

                // Parse the JSON response into a SpotifyTokenResponse object
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true
                };

                var tokenResponse = JsonSerializer.Deserialize<SpotifyTokenResponse>(responseContent, options);

                if (tokenResponse == null)
                {
                    _logger.LogError("Failed to deserialize token response: null result");
                    return null;
                }

                _logger.LogInformation("Token response: AccessToken={HasAccessToken}, RefreshToken={HasRefreshToken}, ExpiresIn={ExpiresIn}",
                    !string.IsNullOrEmpty(tokenResponse.AccessToken),
                    !string.IsNullOrEmpty(tokenResponse.RefreshToken),
                    tokenResponse.ExpiresIn);

                // Verify the access token is actually present
                if (string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    _logger.LogError("Access token is missing in the response despite successful HTTP status");

                    // Try to log property names from response to debug serialization issues
                    try
                    {
                        var jsonDoc = JsonDocument.Parse(responseContent);
                        var properties = string.Join(", ", jsonDoc.RootElement.EnumerateObject().Select(p => p.Name));
                        _logger.LogInformation("Response JSON properties: {Properties}", properties);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to parse response JSON");
                    }

                    return null;
                }

                _logger.LogInformation("Successfully received valid token response with access token length: {TokenLength}",
                    tokenResponse.AccessToken.Length);

                return tokenResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exchanging code for token: {ErrorMessage}", ex.Message);
                return null;
            }
        }

        public async Task<List<SpotifyArtist>> GetTopArtistsAsync(string accessToken, string timeRange = "medium_term", int limit = 10)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentNullException(nameof(accessToken));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync($"me/top/artists?time_range={timeRange}&limit={limit}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var items = json.RootElement.GetProperty("items");

            return items.EnumerateArray().Select(artist =>
            {
                var imageUrl = artist.GetProperty("images").EnumerateArray()
                    .FirstOrDefault().TryGetProperty("url", out var url) ? url.GetString() : null;

                var genres = new List<string>();
                if (artist.TryGetProperty("genres", out var genresElement))
                {
                    genres = genresElement.EnumerateArray()
                        .Select(g => g.GetString())
                        .Where(g => g != null)
                        .ToList();
                }

                return new SpotifyArtist
                {
                    Id = artist.GetProperty("id").GetString(),
                    Name = artist.GetProperty("name").GetString(),
                    ImageUrl = imageUrl,
                    Genres = genres,
                    Popularity = artist.TryGetProperty("popularity", out var pop) ? pop.GetInt32() : 0
                };
            }).ToList();
        }

        public async Task<List<RecentlyPlayedTrack>> GetRecentlyPlayedAsync(string accessToken, int limit = 20)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentNullException(nameof(accessToken));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync($"me/player/recently-played?limit={limit}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var items = json.RootElement.GetProperty("items");

            return items.EnumerateArray().Select(item =>
            {
                var track = item.GetProperty("track");
                var album = track.GetProperty("album");
                var artists = track.GetProperty("artists");
                var playedAt = DateTime.Parse(item.GetProperty("played_at").GetString());

                return new RecentlyPlayedTrack
                {
                    SongName = track.GetProperty("name").GetString(),
                    Artist = string.Join(", ", artists.EnumerateArray().Select(a => a.GetProperty("name").GetString())),
                    Album = album.GetProperty("name").GetString(),
                    AlbumImageUrl = album.GetProperty("images")[0].GetProperty("url").GetString(),
                    SongUrl = track.GetProperty("external_urls").GetProperty("spotify").GetString(),
                    Duration = TimeSpan.FromMilliseconds(track.GetProperty("duration_ms").GetInt32()),
                    PlayedAt = playedAt
                };
            }).ToList();
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

            int rank = 0;
            return items.EnumerateArray().Select(item =>
            {
                var album = item.GetProperty("album");
                var artists = item.GetProperty("artists");
                rank++; // Increment rank for each track

                return new SpotifyDataDTO
                {
                    SongName = item.GetProperty("name").GetString(),
                    Artist = string.Join(", ", artists.EnumerateArray().Select(a => a.GetProperty("name").GetString())),
                    Album = album.GetProperty("name").GetString(),
                    AlbumImageUrl = album.GetProperty("images")[0].GetProperty("url").GetString(),
                    SongUrl = item.GetProperty("external_urls").GetProperty("spotify").GetString(),
                    Duration = TimeSpan.FromMilliseconds(item.GetProperty("duration_ms").GetInt32()),
                    Rank = rank
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

        public async Task<List<SpotifyPlaylistDTO>> GetUserPlaylistsAsync(string accessToken, int limit = 50, bool onlyOwned = false, int topCount = 0)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentNullException(nameof(accessToken));

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Get current user's profile to check playlist ownership later
                var userResponse = await _httpClient.GetAsync("me");
                userResponse.EnsureSuccessStatusCode();
                var userContent = await userResponse.Content.ReadAsStringAsync();
                var userJson = JsonDocument.Parse(userContent);
                string userId = userJson.RootElement.GetProperty("id").GetString();

                // Get user's playlists
                var response = await _httpClient.GetAsync($"me/playlists?limit={limit}");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"Playlists response received with length: {content.Length}");

                var json = JsonDocument.Parse(content);

                // Check if items property exists and is not null
                if (!json.RootElement.TryGetProperty("items", out var itemsElement) || itemsElement.ValueKind == JsonValueKind.Null)
                {
                    _logger.LogWarning("No playlist items found in response or items property is null");
                    return new List<SpotifyPlaylistDTO>();
                }

                var playlists = new List<SpotifyPlaylistDTO>();
                var playlistTasks = new List<Task<SpotifyPlaylistDTO>>();

                // Check that we have an array before trying to iterate
                if (itemsElement.ValueKind == JsonValueKind.Array)
                {
                    int processedCount = 0;

                    // First pass: process playlist basic info and filter if needed
                    foreach (var item in itemsElement.EnumerateArray())
                    {
                        try
                        {
                            // Get basic playlist details
                            string playlistId = item.TryGetProperty("id", out var idProp) ? idProp.GetString() : "";
                            if (string.IsNullOrEmpty(playlistId))
                            {
                                continue;
                            }

                            // Check ownership if filtering is requested
                            string ownerId = "";
                            if (item.TryGetProperty("owner", out var owner) &&
                                owner.TryGetProperty("id", out var ownerIdProp))
                            {
                                ownerId = ownerIdProp.GetString();
                            }

                            bool isOwner = userId == ownerId;

                            // Skip if we only want owned playlists and this isn't owned
                            if (onlyOwned && !isOwner)
                            {
                                continue;
                            }

                            // For the top playlists optimization, we'll get details for all 
                            // potentially relevant playlists and sort later
                            playlistTasks.Add(GetPlaylistDetailsAsync(playlistId, userId, item));
                            processedCount++;

                            // If we only need topCount playlists, limit how many we process
                            // We process a bit more than needed since some might be filtered out
                            if (topCount > 0 && processedCount >= Math.Min(topCount * 2, limit))
                            {
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing playlist item");
                        }
                    }

                    // Wait for all playlist detail requests to complete
                    await Task.WhenAll(playlistTasks);

                    // Collect results
                    foreach (var task in playlistTasks)
                    {
                        if (task.IsCompletedSuccessfully && task.Result != null)
                        {
                            playlists.Add(task.Result);
                        }
                    }

                    // Sort by saves count
                    playlists = playlists.OrderByDescending(p => p.SavesCount).ToList();

                    // Limit to topCount if specified
                    if (topCount > 0 && playlists.Count > topCount)
                    {
                        playlists = playlists.Take(topCount).ToList();
                    }
                }

                return playlists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user playlists");
                throw;
            }
        }

        // Helper method to get details for a single playlist
        private async Task<SpotifyPlaylistDTO> GetPlaylistDetailsAsync(string playlistId, string userId, JsonElement basicInfo)
        {
            try
            {
                // Get additional playlist details
                var playlistResponse = await _httpClient.GetAsync($"playlists/{playlistId}");
                if (!playlistResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to get details for playlist {playlistId}: {playlistResponse.StatusCode}");
                    return null;
                }

                var playlistContent = await playlistResponse.Content.ReadAsStringAsync();
                var playlistJson = JsonDocument.Parse(playlistContent);

                // Extract values with null checks
                int savesCount = 0;
                if (playlistJson.RootElement.TryGetProperty("followers", out var followers) &&
                    followers.TryGetProperty("total", out var total))
                {
                    savesCount = total.GetInt32();
                }

                string ownerName = "";
                string ownerId = "";
                bool isOwner = false;

                if (playlistJson.RootElement.TryGetProperty("owner", out var owner))
                {
                    ownerName = owner.TryGetProperty("display_name", out var displayName) ?
                        displayName.GetString() : "";

                    ownerId = owner.TryGetProperty("id", out var ownerIdProp) ?
                        ownerIdProp.GetString() : "";

                    isOwner = userId == ownerId;
                }

                string name = basicInfo.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : "Untitled Playlist";
                string description = basicInfo.TryGetProperty("description", out var descProp) ? descProp.GetString() : "";

                // Handle images safely
                string imageUrl = null;
                if (basicInfo.TryGetProperty("images", out var images) &&
                    images.ValueKind == JsonValueKind.Array &&
                    images.GetArrayLength() > 0 &&
                    images[0].TryGetProperty("url", out var url))
                {
                    imageUrl = url.GetString();
                }

                // Handle external URLs safely
                string externalUrl = null;
                if (basicInfo.TryGetProperty("external_urls", out var urls) &&
                    urls.TryGetProperty("spotify", out var spotifyUrl))
                {
                    externalUrl = spotifyUrl.GetString();
                }

                // Handle tracks count safely
                int tracksCount = 0;
                if (basicInfo.TryGetProperty("tracks", out var tracks) &&
                    tracks.TryGetProperty("total", out var tracksTotal))
                {
                    tracksCount = tracksTotal.GetInt32();
                }

                return new SpotifyPlaylistDTO
                {
                    Id = playlistId,
                    Name = name,
                    Description = description,
                    ImageUrl = imageUrl,
                    TracksCount = tracksCount,
                    SavesCount = savesCount,
                    IsOwner = isOwner,
                    Owner = ownerName,
                    ExternalUrl = externalUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting details for playlist {playlistId}");
                return null;
            }
        }
    }



    public class SpotifyArtist
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public List<string> Genres { get; set; } = new List<string>();
        public int Popularity { get; set; }
    }

    // Define this class if not already defined elsewhere
    public class RecentlyPlayedTrack : SpotifyDataDTO
    {
        public DateTime PlayedAt { get; set; }
    }
}
