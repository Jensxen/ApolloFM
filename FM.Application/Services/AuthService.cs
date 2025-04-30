using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Json;
using FM.Application.Services.ServiceDTO;
using Microsoft.Extensions.DependencyInjection;

namespace FM.Application.Services
{
    public class AuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly NavigationManager _navigationManager;
        private readonly ILocalStorageService _localStorage;
        private readonly bool _isBlazorContext;

        // Opdateret konstruktør uden tokenProvider
        [ActivatorUtilitiesConstructor]
        public AuthService(
            IHttpClientFactory httpClientFactory,
            AuthenticationStateProvider authStateProvider,
            NavigationManager navigationManager,
            ILocalStorageService localStorage)
        {
            _httpClientFactory = httpClientFactory;
            _authStateProvider = authStateProvider;
            _navigationManager = navigationManager;
            _localStorage = localStorage;
            _isBlazorContext = true;
        }

        // API Constructor (not used in this implementation)
        public AuthService(HttpClient httpClient)
        {
            _httpClientFactory = null;
            _authStateProvider = null;
            _navigationManager = null;
            _localStorage = null;
            _isBlazorContext = false;
        }

        public async Task<bool> IsUserAuthenticated()
        {
            if (!_isBlazorContext)
                throw new InvalidOperationException("This method is only available in Blazor context.");

            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            return authState.User.Identity?.IsAuthenticated ?? false;
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

        public async Task Logout()
        {
            if (!_isBlazorContext)
                throw new InvalidOperationException("This method is only available in Blazor context.");

            // Fjern token fra localStorage direkte i stedet for at gå gennem AuthStateProvider
            await _localStorage.RemoveItemAsync("spotify_access_token");

            // Naviger til forsiden
            _navigationManager.NavigateTo("/");
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
                var response = await http.GetFromJsonAsync<SpotifyDataDTO>("api/auth/currently-playing");
                return response;
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

        // Helper method to ensure token is in the header
        private async Task EnsureTokenInHeaderAsync(HttpClient client)
        {
            if (!_isBlazorContext)
                return;


            var token = await _localStorage.GetItemAsync<string>("spotify_access_token");

            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("No access token available");
            }

            // Set the Authorization header
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // I FM.Application/Services/AuthService.cs
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
