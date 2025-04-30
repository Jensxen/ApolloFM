// Add this to a new file: ApolloFM/JsInterop/AuthInterop.cs
using Microsoft.JSInterop;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System;

namespace ApolloFM.JsInterop
{
    public static class AuthInterop
    {
        private static AuthenticationStateProvider _authStateProvider;
        private static NavigationManager _navigationManager;

        public static void Initialize(AuthenticationStateProvider authStateProvider, NavigationManager navigationManager)
        {
            _authStateProvider = authStateProvider;
            _navigationManager = navigationManager;
        }

        [JSInvokable]
        public static async Task ForceAuthenticationStateRefresh()
        {
            try
            {
                if (_authStateProvider is SpotifyAuthenticationStateProvider spotifyAuth)
                {
                    spotifyAuth.TriggerAuthenticationStateChanged();
                    Console.WriteLine("Authentication state refresh triggered from JavaScript");
                }
                else
                {
                    Console.WriteLine("Auth provider not initialized or wrong type");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error in ForceAuthenticationStateRefresh: {ex.Message}");
            }
        }

        [JSInvokable]
        public static void NavigateToDashboard()
        {
            try
            {
                if (_navigationManager != null)
                {
                    _navigationManager.NavigateTo("/dashboard");
                    Console.WriteLine("Navigation to dashboard triggered from JavaScript");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error navigating to dashboard: {ex.Message}");
            }
        }
    }
}
