// Make sure this class correctly implements AuthenticationStateProvider
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Security.Claims;

public class SpotifyAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;
    private readonly NavigationManager _navigationManager;
    private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

    public SpotifyAuthenticationStateProvider(IJSRuntime jsRuntime, NavigationManager navigationManager)
    {
        _jsRuntime = jsRuntime;
        _navigationManager = navigationManager;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_currentUser));
    }

    public async Task MarkUserAsAuthenticated(string token)
    {
        try
        {
            var authenticatedUser = new ClaimsPrincipal(
                new ClaimsIdentity(
                    GetClaimsFromToken(token),
                    "spotify"
                )
            );

            _currentUser = authenticatedUser;
            NotifyAuthenticationStateChanged(
                Task.FromResult(new AuthenticationState(_currentUser))
            );
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error marking user as authenticated: {ex.Message}");
            throw;
        }
    }

    public Task MarkUserAsLoggedOut()
    {
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(_currentUser))
        );

        return Task.CompletedTask;
    }

    public void TriggerAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(_currentUser))
        );
        Console.WriteLine("Authentication state change triggered");
    }

    private IEnumerable<Claim> GetClaimsFromToken(string token)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "spotify_user"),
            new Claim(ClaimTypes.Name, "Spotify User"),
            new Claim(ClaimTypes.Authentication, token)
        };

        return claims;
    }
}
