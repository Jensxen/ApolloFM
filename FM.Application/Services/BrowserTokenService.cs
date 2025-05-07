using FM.Application.Interfaces.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

public class BrowserTokenService : ITokenService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<BrowserTokenService> _logger;

    // Constants for localStorage keys
    private const string ACCESS_TOKEN_KEY = "spotify_access_token";
    private const string REFRESH_TOKEN_KEY = "spotify_refresh_token";
    private const string TOKEN_EXPIRY_KEY = "spotify_token_expiry";

    public BrowserTokenService(IJSRuntime jsRuntime, ILogger<BrowserTokenService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task<string> GetAccessToken()
    {
        try 
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", ACCESS_TOKEN_KEY);
            _logger.LogDebug("Retrieved access token from localStorage: {HasToken}", !string.IsNullOrEmpty(token));
            return token;
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, "Error retrieving access token from localStorage");
            return null;
        }
    }

    public async Task SetAccessToken(string accessToken, int expiresInSeconds)
    {
        if (string.IsNullOrEmpty(accessToken))
            return;
        
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", ACCESS_TOKEN_KEY, accessToken);
            
            // Calculate and store expiration time
            var expirationTime = DateTime.UtcNow.AddSeconds(expiresInSeconds);
            await _jsRuntime.InvokeVoidAsync(
                "localStorage.setItem", 
                TOKEN_EXPIRY_KEY, 
                expirationTime.ToString("o")
            );
            
            _logger.LogDebug("Access token stored in localStorage, expires in {Seconds} seconds", expiresInSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing access token in localStorage");
        }
    }

    public async Task<string> GetRefreshToken()
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", REFRESH_TOKEN_KEY);
            _logger.LogDebug("Retrieved refresh token from localStorage: {HasToken}", !string.IsNullOrEmpty(token));
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving refresh token from localStorage");
            return null;
        }
    }

    public async Task SetRefreshToken(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            return;
        
        try 
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", REFRESH_TOKEN_KEY, refreshToken);
            await _jsRuntime.InvokeVoidAsync(
                "localStorage.setItem", 
                "spotify_refresh_token_timestamp", 
                DateTime.UtcNow.ToString("o")
            );
            _logger.LogInformation("Refresh token stored in localStorage");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing refresh token in localStorage");
        }
    }

    public async Task<bool> NeedsRefresh()
    {
        try
        {
            var expiryString = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", TOKEN_EXPIRY_KEY);
            
            if (string.IsNullOrEmpty(expiryString))
                return true;
                
            if (DateTime.TryParse(expiryString, out var expiry))
            {
                // Refresh if token expires in less than 5 minutes
                bool needsRefresh = expiry < DateTime.UtcNow.AddMinutes(5);
                _logger.LogDebug("Token needs refresh: {NeedsRefresh}, expires at: {ExpiryTime}", 
                    needsRefresh, expiry);
                return needsRefresh;
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if token needs refresh");
            return true;
        }
    }

    public async Task ClearAccessTokens()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", ACCESS_TOKEN_KEY);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TOKEN_EXPIRY_KEY);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", REFRESH_TOKEN_KEY);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "spotify_auth_state");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "spotify_login_timestamp");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "spotify_refresh_token_timestamp");
            _logger.LogInformation("All tokens cleared from localStorage");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing tokens from localStorage");
        }
    }
}
