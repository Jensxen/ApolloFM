using FM.Domain.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;

namespace FM.Application.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly NavigationManager _navigationManager;
        private readonly bool _isBlazorContext;

        // Blazor WebAssembly client constructor
        public AuthService(
            HttpClient httpClient, 
            AuthenticationStateProvider authStateProvider, 
            NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
            _navigationManager = navigationManager;
            _isBlazorContext = true;
        }

        // API Constructor
        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _authStateProvider = null;
            _navigationManager = null;
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
                return await _httpClient.GetFromJsonAsync<SpotifyUserProfile>("api/auth/user");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving user profile: {ex.Message}");
                return null;
            }
        }

        public void Logout()
        {
            if (!_isBlazorContext)
                throw new InvalidOperationException("This method is only available in Blazor context.");
                
            _navigationManager.NavigateTo("api/auth/logout", forceLoad: true);
        }
    }
}
