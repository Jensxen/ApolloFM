using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.JSInterop;
using System.Text.Json;
using FM.Application.Services.ServiceDTO.SpotifyDTO;

namespace FM.Application.Services.AuthServices
{
    public class AuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly NavigationManager _navigationManager;
        private readonly TokenService _tokenService;
        private readonly IJSRuntime _jsRuntime;

        public AuthService(
            IHttpClientFactory httpClientFactory,
            AuthenticationStateProvider authStateProvider,
            NavigationManager navigationManager,
            TokenService tokenService,
            IJSRuntime jsRuntime)
        {
            _httpClientFactory = httpClientFactory;
            _authStateProvider = authStateProvider;
            _navigationManager = navigationManager;
            _tokenService = tokenService;
            _jsRuntime = jsRuntime;
        }

        public async Task<bool> IsUserAuthenticated()
        {
            var token = _tokenService.GetAccessToken();
            return !string.IsNullOrEmpty(token);
        }

        public async Task<SpotifyUserProfile?> GetUserProfile()
        {
            try
            {
                // Use GetApiDataAsync instead of direct client.GetAsync to ensure proper auth headers
                var userProfile = await GetApiDataAsync<SpotifyUserProfile>("api/auth/user");
                return userProfile;
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Error fetching user profile: {ex.Message}");
                return null;
            }
        }

        public async Task<List<SpotifyArtist>> GetTopArtistsAsync(string timeRange = "medium_term")
        {
            try
            {
                return await GetApiDataAsync<List<SpotifyArtist>>($"api/auth/top-artists?timeRange={timeRange}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting top artists: {ex.Message}");
                return new List<SpotifyArtist>();
            }
        }

        public async Task<List<RecentlyPlayedTrack>> GetRecentlyPlayedAsync(int limit = 20)
        {
            try
            {
                return await GetApiDataAsync<List<RecentlyPlayedTrack>>($"api/auth/recently-played?limit={limit}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting recently played: {ex.Message}");
                return new List<RecentlyPlayedTrack>();
            }
        }

        public async Task<List<SpotifyDataDTO>> GetTopTracksAsync(string timeRange = "medium_term")
        {
            try
            {
                return await GetApiDataAsync<List<SpotifyDataDTO>>($"api/auth/top-tracks?timeRange={timeRange}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting top tracks: {ex.Message}");
                throw;
            }
        }


        public async Task<SpotifyDataDTO?> GetCurrentlyPlayingTrackAsync()
        {
            try
            {
                return await GetApiDataAsync<SpotifyDataDTO>("api/auth/currently-playing");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                // No content means no track is playing
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting currently playing track: {ex.Message}");
                throw;
            }
        }

        public async Task LogoutAsync()
        {
            // Use the correct method name from your TokenService class
            _tokenService.ClearAccessTokens(); // or ClearTokens() depending on your implementation

            var authProvider = (SpotifyAuthenticationStateProvider)_authStateProvider;
            await authProvider.MarkUserAsLoggedOut();

            _navigationManager.NavigateTo("/");
        }

        public async Task LoginAsync()
        {
            try
            {
                var apiUrl = "https://localhost:7043"; // Your API URL

                // Generate a random state value for security
                var state = Guid.NewGuid().ToString();

                // Store it in localStorage for validation later
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "spotify_auth_state", state);

                // This should be the Blazor app's spotify-callback component URL
                var redirectUri = $"{_navigationManager.BaseUri}spotify-callback";

                // Use the client app's callback component and pass state
                var loginUrl = $"{apiUrl}/api/auth/login?returnUrl={Uri.EscapeDataString(redirectUri)}&clientState={Uri.EscapeDataString(state)}";

                Console.WriteLine($"Redirecting to login URL: {loginUrl}");
                _navigationManager.NavigateTo(loginUrl, forceLoad: true);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Login error: {ex.Message}");
                throw;
            }
        }

        public async Task HandleCallback(string code)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    Console.Error.WriteLine("Cannot handle callback: authorization code is empty");
                    _navigationManager.NavigateTo("/?error=missing_code");
                    return;
                }

                // Log the code we're using
                Console.WriteLine($"Handling callback with code: {code.Substring(0, Math.Min(10, code.Length))}...");

                // Create client for API requests
                var client = _httpClientFactory.CreateClient("ApolloAPI");

                // Send code in JSON body using POST
                var responseMessage = await client.PostAsJsonAsync("api/auth/handle-state-error", new { code });

                // Log the status
                Console.WriteLine($"POST /api/auth/handle-state-error status: {(int)responseMessage.StatusCode} {responseMessage.StatusCode}");

                if (responseMessage.IsSuccessStatusCode)
                {
                    var response = await responseMessage.Content.ReadFromJsonAsync<TokenResponse>();

                    if (response != null && !string.IsNullOrEmpty(response.AccessToken))
                    {
                        // Store tokens
                        _tokenService.SetAccessToken(response.AccessToken, response.ExpiresIn);

                        if (!string.IsNullOrEmpty(response.RefreshToken))
                        {
                            _tokenService.SetRefreshToken(response.RefreshToken);
                        }

                        // Update auth state
                        var authProvider = (SpotifyAuthenticationStateProvider)_authStateProvider;
                        await authProvider.MarkUserAsAuthenticated(response.AccessToken);

                        Console.WriteLine("Authentication successful, redirecting to dashboard...");

                        _navigationManager.NavigateTo("/dashboard");
                    }
                    else
                    {
                        Console.Error.WriteLine("Failed to retrieve valid token from API response");
                        _navigationManager.NavigateTo("/?error=invalid_token_response");
                    }
                }
                else
                {
                    Console.Error.WriteLine($"Token request failed: {responseMessage.StatusCode}");
                    _navigationManager.NavigateTo("/?error=token_request_failed");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error handling callback: {ex.Message}");
                _navigationManager.NavigateTo("/?error=authentication_failed");
            }
        }

        private async Task<T> GetApiDataAsync<T>(string endpoint)
        {
            try
            {
                // Get a valid access token (with automatic refresh if needed)
                var token = await GetValidAccessTokenAsync();

                await _jsRuntime.InvokeVoidAsync("console.log",
                    $"Making API request to {endpoint} with token: {(token != null ? $"{token.Substring(0, 10)}..." : "null")}");

                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No access token available");
                }

                var client = _httpClientFactory.CreateClient("ApolloAPI");

                // Add the authorization header
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Log the headers for debugging
                await _jsRuntime.InvokeVoidAsync("console.log",
                    "Authorization header set: " + client.DefaultRequestHeaders.Authorization?.ToString());

                var response = await client.GetAsync(endpoint);

                // Log response status
                await _jsRuntime.InvokeVoidAsync("console.log",
                    $"API response: {(int)response.StatusCode} {response.StatusCode}");

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"API request error on {endpoint}: {ex.Message}");
                throw;
            }
        }

        public async Task<string> GetValidAccessTokenAsync()
        {
            var token = _tokenService.GetAccessToken();

            // If token is missing or needs refresh
            if (string.IsNullOrEmpty(token) || _tokenService.NeedsRefresh())
            {
                var refreshToken = _tokenService.GetRefreshToken();
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    try
                    {
                        // Create client for API requests
                        var client = _httpClientFactory.CreateClient("ApolloAPI");

                        // Call your API to refresh the token
                        var response = await client.PostAsync(
                            $"api/auth/refresh?refreshToken={Uri.EscapeDataString(refreshToken)}",
                            null);

                        if (response.IsSuccessStatusCode)
                        {
                            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
                            if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.AccessToken))
                            {
                                _tokenService.SetAccessToken(tokenResponse.AccessToken, tokenResponse.ExpiresIn);

                                if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
                                {
                                    _tokenService.SetRefreshToken(tokenResponse.RefreshToken);
                                }

                                token = tokenResponse.AccessToken;
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine($"Failed to refresh token: {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error refreshing token: {ex.Message}");
                    }
                }
            }

            return token;
        }

        public class TokenResponse
        {
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
            public int ExpiresIn { get; set; }
        }

        public class SpotifyArtist
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string ImageUrl { get; set; }
            public List<string> Genres { get; set; } = new List<string>();
            public int Popularity { get; set; }
        }
        public class RecentlyPlayedTrack : SpotifyDataDTO
        {
            public DateTime PlayedAt { get; set; }
        }


    }
}
