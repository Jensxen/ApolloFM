using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using FM.Application.Services.ServiceDTO;
using System.Net.Http.Json;

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

        public async Task Logout()
        {
            _tokenService.ClearAccessToken();
            _navigationManager.NavigateTo("/");
        }

        private async Task EnsureTokenInHeaderAsync(HttpClient client)
        {
            var token = _tokenService.GetAccessToken();

            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("No access token available");
            }

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<SpotifyUserProfile> GetUserProfile()
        {
            try
            {
                var http = _httpClientFactory.CreateClient("ApolloAPI");
                await EnsureTokenInHeaderAsync(http);
                return await http.GetFromJsonAsync<SpotifyUserProfile>("api/auth/user");
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
                var http = _httpClientFactory.CreateClient("ApolloAPI");
                await EnsureTokenInHeaderAsync(http);
                return await http.GetFromJsonAsync<List<SpotifyDataDTO>>($"api/auth/top-tracks?timeRange={timeRange}");
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
                var http = _httpClientFactory.CreateClient("ApolloAPI");
                await EnsureTokenInHeaderAsync(http);
                return await http.GetFromJsonAsync<SpotifyDataDTO>("api/auth/currently-playing");
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

        public void Login(NavigationManager navigationManager, bool useDirectMethod = false)
        {
            var apiUrl = "https://localhost:7043"; // Kan evt. gemmes i konfiguration
            var returnUrl = Uri.EscapeDataString($"{navigationManager.BaseUri}dashboard");

            string loginUrl;
            if (useDirectMethod)
            {
                // Brug den direkte metode, der virker via handle-state-error
                loginUrl = $"{apiUrl}/api/auth/handle-state-error";
                Console.WriteLine($"AuthService: Using direct login with handle-state-error");
            }
            else
            {
                // Brug standard OAuth flow
                loginUrl = $"{apiUrl}/api/auth/login?returnUrl={returnUrl}";
                Console.WriteLine($"AuthService: Using standard login flow with returnUrl: {returnUrl}");
            }

            Console.WriteLine($"AuthService: Navigating to {loginUrl}");
            navigationManager.NavigateTo(loginUrl, forceLoad: true);
        }


    }
}
