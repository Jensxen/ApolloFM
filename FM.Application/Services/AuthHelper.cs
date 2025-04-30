using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace FM.Application.Services
{
    public static class AuthHelpers
    {
        private static AuthenticationStateProvider _authStateProvider;

        // Initialiser AuthHelpers med en AuthenticationStateProvider
        public static void Initialize(AuthenticationStateProvider authStateProvider)
        {
            _authStateProvider = authStateProvider;
        }

        // Force en opdatering af authentication state
        [JSInvokable("ForceAuthenticationStateRefresh")]
        public static async Task ForceAuthenticationStateRefresh()
        {
            if (_authStateProvider is SpotifyAuthenticationStateProvider spotifyAuth)
            {
                Console.WriteLine("AuthHelpers: Forcing authentication state refresh");
                await spotifyAuth.NotifyAuthenticationStateChanged();
            }
            else
            {
                Console.WriteLine("AuthHelpers: AuthenticationStateProvider is not SpotifyAuthenticationStateProvider");
            }
        }
    }
}
