using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FM.Application.Services
{
    public class SpotifyAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly NavigationManager _navigationManager;

        public SpotifyAuthenticationStateProvider(ILocalStorageService localStorage, NavigationManager navigationManager)
        {
            _localStorage = localStorage;
            _navigationManager = navigationManager;
            Console.WriteLine("SpotifyAuthenticationStateProvider instantiated");
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            Console.WriteLine("SpotifyAuthenticationStateProvider: Getting authentication state");

            try
            {
                var token = await _localStorage.GetItemAsync<string>("spotify_access_token");
                Console.WriteLine($"SpotifyAuthenticationStateProvider: Token exists? {!string.IsNullOrEmpty(token)}");

                if (!string.IsNullOrEmpty(token))
                {
                    Console.WriteLine($"SpotifyAuthenticationStateProvider: Token starts with: {token.Substring(0, Math.Min(10, token.Length))}");

                    var identity = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, "Spotify User"),
                        new Claim("access_token", token)
                    }, "Spotify Authentication");

                    var user = new ClaimsPrincipal(identity);
                    return new AuthenticationState(user);
                }

                Console.WriteLine("SpotifyAuthenticationStateProvider: No token, returning unauthenticated");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SpotifyAuthenticationStateProvider error: {ex.Message}");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public async Task MarkUserAsAuthenticated(string token)
        {
            Console.WriteLine("SpotifyAuthenticationStateProvider: MarkUserAsAuthenticated called");

            try
            {
                // Store the token in localStorage
                await _localStorage.SetItemAsync("spotify_access_token", token);
                Console.WriteLine("SpotifyAuthenticationStateProvider: Token stored in localStorage");

                // Create a claims identity with the token
                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, "Spotify User"),
                    new Claim("access_token", token)
                }, "Spotify Authentication");

                var user = new ClaimsPrincipal(identity);

                // Notify the UI that authentication state has changed
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
                Console.WriteLine("SpotifyAuthenticationStateProvider: Authentication state updated");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SpotifyAuthenticationStateProvider error in MarkUserAsAuthenticated: {ex.Message}");
            }
        }

        public async Task MarkUserAsLoggedOut()
        {
            Console.WriteLine("SpotifyAuthenticationStateProvider: MarkUserAsLoggedOut called");

            try
            {
                await _localStorage.RemoveItemAsync("spotify_access_token");
                Console.WriteLine("SpotifyAuthenticationStateProvider: Token removed from localStorage");

                var identity = new ClaimsIdentity();
                var user = new ClaimsPrincipal(identity);
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
                Console.WriteLine("SpotifyAuthenticationStateProvider: User marked as logged out");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SpotifyAuthenticationStateProvider error in MarkUserAsLoggedOut: {ex.Message}");
            }
        }
    }
}
