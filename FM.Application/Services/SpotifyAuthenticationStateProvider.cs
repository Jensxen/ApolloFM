using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Threading.Tasks;

public class SpotifyAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;
    private readonly NavigationManager _navigation;
    private ClaimsPrincipal _user = new ClaimsPrincipal(new ClaimsIdentity());

    private const string TokenKey = "spotify_token";

    public SpotifyAuthenticationStateProvider(IJSRuntime jsRuntime, NavigationManager navigation)
    {
        _jsRuntime = jsRuntime;
        _navigation = navigation;

        Console.WriteLine("SpotifyAuthenticationStateProvider instantiated");
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        Console.WriteLine("SpotifyAuthenticationStateProvider: Getting authentication state");

        var token = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", TokenKey);

        var identity = string.IsNullOrEmpty(token)
            ? new ClaimsIdentity()
            : new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "SpotifyUser"),
                new Claim("access_token", token)
            }, "Spotify");

        _user = new ClaimsPrincipal(identity);

        return new AuthenticationState(_user);
    }

    public async Task MarkUserAsAuthenticated(string token)
    {
        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", TokenKey, token);

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "SpotifyUser"),
            new Claim("access_token", token)
        }, "Spotify");

        _user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_user)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", TokenKey);

        _user = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_user)));
    }

    public async Task<string> GetAccessTokenAsync()
    {
        return await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", TokenKey);
    }

    public void TriggerAuthenticationStateChanged()
{
    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}

}
