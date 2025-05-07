using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using FM.Application.Services.ServiceDTO;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.JSInterop;
using System.ComponentModel.Design;
using FM.Application.Interfaces.IServices;

namespace FM.Application.Services
{
    public class BrowserAuthService : IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly NavigationManager _navigationManager;
        private readonly ITokenService _tokenService;
        private readonly IJSRuntime _jsRuntime;

        public BrowserAuthService(
            IHttpClientFactory httpClientFactory,
            AuthenticationStateProvider authStateProvider,
            NavigationManager navigationManager,
            ITokenService tokenService,
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
            var token = await _tokenService.GetAccessToken();
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
            await _tokenService.ClearAccessTokens();

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

                await _jsRuntime.InvokeVoidAsync("console.log", $"Redirecting to login URL: {loginUrl}");
                _navigationManager.NavigateTo(loginUrl, forceLoad: true);
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Login error: {ex.Message}");
                throw;
            }
        }

        public async Task HandleCallback(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentException("Authorization code is empty");
            }

            await _jsRuntime.InvokeVoidAsync("console.log",
                $"Processing authorization code at {DateTime.UtcNow.ToString("HH:mm:ss.fff")} UTC");
            await _jsRuntime.InvokeVoidAsync("console.log", $"Code length: {code.Length}");

            try
            {
                // Create client for API requests
                var client = _httpClientFactory.CreateClient("ApolloAPI");

                // Important: Set a reasonable timeout
                client.Timeout = TimeSpan.FromSeconds(30);

                // Add timestamp to request for debugging
                var payload = new
                {
                    Code = code,
                    Timestamp = DateTime.UtcNow.ToString("o")
                };

                // Log the API endpoint we're calling
                var endpoint = "api/auth/handle-state-error";
                await _jsRuntime.InvokeVoidAsync("console.log", $"Calling API endpoint: {endpoint}");

                // Send code in JSON body using POST
                var response = await client.PostAsJsonAsync(endpoint, payload);

                // Check if request succeeded
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    await _jsRuntime.InvokeVoidAsync("console.error",
                        $"Server returned error {response.StatusCode}: {errorContent}");

                    throw new Exception($"Token exchange failed: {response.StatusCode}");
                }

                // Process successful response
                var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    await _jsRuntime.InvokeVoidAsync("console.error", "Server returned invalid token response");
                    throw new Exception("Invalid token response from server");
                }

                // Store tokens in browser storage
                await _tokenService.SetAccessToken(tokenResponse.AccessToken, tokenResponse.ExpiresIn);

                if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
                {
                    await _tokenService.SetRefreshToken(tokenResponse.RefreshToken);
                    await _jsRuntime.InvokeVoidAsync("console.log", "Refresh token stored successfully");
                }
                else
                {
                    await _jsRuntime.InvokeVoidAsync("console.warn", "No refresh token received from server");
                }

                // Update auth state
                var authProvider = (SpotifyAuthenticationStateProvider)_authStateProvider;
                await authProvider.MarkUserAsAuthenticated(tokenResponse.AccessToken);

                await _jsRuntime.InvokeVoidAsync("console.log", "Authentication completed successfully");

                // Store success flag to prevent reprocessing
                await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem",
                    "auth_success_timestamp", DateTime.UtcNow.ToString("o"));
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Error during API call: {ex.Message}");
                throw;
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
            try
            {
                // Get the current access token
                var accessToken = await _tokenService.GetAccessToken();

                // Check if we have a token and if it's still valid
                if (!string.IsNullOrEmpty(accessToken) && !(await _tokenService.NeedsRefresh()))
                {
                    await _jsRuntime.InvokeVoidAsync("console.log", "Using existing valid access token");
                    return accessToken;
                }

                // Token is missing or expired, try to refresh it
                var refreshToken = await _tokenService.GetRefreshToken();

                if (string.IsNullOrEmpty(refreshToken))
                {
                    await _jsRuntime.InvokeVoidAsync("console.warn", "No refresh token available");
                    return null;
                }

                await _jsRuntime.InvokeVoidAsync("console.log", "Refreshing access token using refresh token");

                // Call API to refresh the token
                var client = _httpClientFactory.CreateClient("ApolloAPI");
                var refreshUrl = $"api/auth/refresh?refreshToken={Uri.EscapeDataString(refreshToken)}";

                var response = await client.PostAsync(refreshUrl, null);

                if (response.IsSuccessStatusCode)
                {
                    var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

                    if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.AccessToken))
                    {
                        // Log successful token refresh
                        await _jsRuntime.InvokeVoidAsync("console.log", "Token refreshed successfully");

                        // Store the new access token
                        await _tokenService.SetAccessToken(tokenResponse.AccessToken, tokenResponse.ExpiresIn);

                        // Store the new refresh token if one was provided
                        if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
                        {
                            await _tokenService.SetRefreshToken(tokenResponse.RefreshToken);
                        }

                        // Update authentication state
                        var authProvider = (SpotifyAuthenticationStateProvider)_authStateProvider;
                        await authProvider.MarkUserAsAuthenticated(tokenResponse.AccessToken);

                        return tokenResponse.AccessToken;
                    }
                }
                else
                {
                    // Log the failed refresh attempt
                    var errorContent = await response.Content.ReadAsStringAsync();
                    await _jsRuntime.InvokeVoidAsync("console.error",
                        $"Token refresh failed: {response.StatusCode}, Error: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Error during token refresh: {ex.Message}");
            }

            // If we get here, token refresh failed
            return null;
        }

        public class TokenResponse
        {
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
            public int ExpiresIn { get; set; }
        }


    }
}
