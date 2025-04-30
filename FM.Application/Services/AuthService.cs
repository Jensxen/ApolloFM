using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using FM.Application.Services.ServiceDTO;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace FM.Application.Services
{
    public class AuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly NavigationManager _navigationManager;
        private readonly TokenService _tokenService;

        public AuthService(
            IHttpClientFactory httpClientFactory,
            AuthenticationStateProvider authStateProvider,
            NavigationManager navigationManager,
            TokenService tokenService)
        {
            _httpClientFactory = httpClientFactory;
            _authStateProvider = authStateProvider;
            _navigationManager = navigationManager;
            _tokenService = tokenService;
        }

        public async Task<bool> IsUserAuthenticated()
        {
            var token = _tokenService.GetAccessToken();
            return !string.IsNullOrEmpty(token);
        }

        public async Task<SpotifyUserProfile> GetUserProfile()
        {
            try
            {
                return await GetApiDataAsync<SpotifyUserProfile>("api/auth/user");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving user profile: {ex.Message}");
                return null;
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
            var clientId = "824cb0e5e1d549c683c642d9c9ae062b";
            var redirectUri = $"{_navigationManager.BaseUri}spotify-callback"; // Create this page
            var scopes = "user-read-private user-read-email user-top-read user-read-currently-playing";

            var spotifyUrl = $"https://accounts.spotify.com/authorize" +
                             $"?client_id={Uri.EscapeDataString(clientId)}" +
                             $"&response_type=code" +
                             $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                             $"&scope={Uri.EscapeDataString(scopes)}";

            _navigationManager.NavigateTo(spotifyUrl, forceLoad: true);
        }

        public async Task HandleCallback(string code)
        {
            try
            {
                // Base URL for your API
                var apiUrl = "https://localhost:7043";
                var clientId = "824cb0e5e1d549c683c642d9c9ae062b";

                // Create client for API requests
                var client = _httpClientFactory.CreateClient("ApolloAPI");

                // Get token from API
                var response = await client.GetFromJsonAsync<TokenResponse>($"api/auth/handle-state-error?code={Uri.EscapeDataString(code)}");

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

                    // Redirect to dashboard
                    _navigationManager.NavigateTo("/dashboard");
                }
                else
                {
                    // Handle error
                    _navigationManager.NavigateTo("/?error=token_retrieval_failed");
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
            // Get a valid access token (with automatic refresh if needed)
            var token = await GetValidAccessTokenAsync();

            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("No access token available");
            }

            var client = _httpClientFactory.CreateClient("ApolloAPI");

            // Add the authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>();
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


    }
}
