using AspNet.Security.OAuth.Spotify;
using FM.Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using SpotifyAPI.Web;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly SpotifyService _spotifyService;
    private readonly IConfiguration _configuration;
    private readonly TokenService _tokenService;
    private readonly string _clientAppUrl;

    public AuthController(
        ILogger<AuthController> logger,
        SpotifyService spotifyService,
        IConfiguration configuration,
        TokenService tokenService)
    {
        _logger = logger;
        _spotifyService = spotifyService;
        _configuration = configuration;
        _tokenService = tokenService;
        _clientAppUrl = _configuration["AppSettings:ClientAppUrl"] ?? "https://localhost:7210";

        _logger.LogInformation("ClientAppUrl configured as: {ClientAppUrl}", _clientAppUrl);
    }

    private async Task<string> GetValidAccessTokenAsync()
    {
        // Check if we have a valid access token
        var accessToken = _tokenService.GetAccessToken();

        if (!string.IsNullOrEmpty(accessToken) && !_tokenService.NeedsRefresh())
        {
            return accessToken;
        }

        // If not, try to refresh using the refresh token
        var refreshToken = _tokenService.GetRefreshToken();

        if (string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("No refresh token available to refresh the access token");
            return null;
        }

        try
        {
            var clientId = _configuration["Spotify:ClientId"];
            var clientSecret = _configuration["Spotify:ClientSecret"];

            var newTokenResponse = await _spotifyService.RefreshAccessTokenAsync(refreshToken, clientId, clientSecret);

            if (newTokenResponse != null && !string.IsNullOrEmpty(newTokenResponse.AccessToken))
            {
                _tokenService.SetAccessToken(newTokenResponse.AccessToken, newTokenResponse.ExpiresIn);

                if (!string.IsNullOrEmpty(newTokenResponse.RefreshToken))
                {
                    _tokenService.SetRefreshToken(newTokenResponse.RefreshToken);
                }

                _logger.LogInformation("Successfully refreshed access token");
                return newTokenResponse.AccessToken;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing access token");
        }

        return null;
    }

    [HttpGet("user")]
    public async Task<IActionResult> GetUser()
    {
        // Don't rely on User.Identity.IsAuthenticated for API endpoints with token-based auth
        // Instead, directly try to get a valid access token
        var token = await GetValidAccessTokenAsync();

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("No valid access token found for user info request");
            return Unauthorized("Access token is missing or invalid");
        }

        try
        {
            var config = SpotifyClientConfig.CreateDefault().WithToken(token);
            var spotify = new SpotifyClient(config);
            var user = await spotify.UserProfile.Current();

            _logger.LogInformation("Successfully retrieved user data for {UserId}", user.Id);

            return Ok(new
            {
                user.DisplayName,
                user.Email,
                user.Id,
                user.Images
            });
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogError("Unauthorized when fetching user data: {Message}", ex.Message);
            _tokenService.ClearAccessTokens();
            return Unauthorized("The access token is invalid or expired");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user data from Spotify");
            return BadRequest($"Error fetching user data: {ex.Message}");
        }
    }

    [HttpGet("top-tracks/all-time")]
    public async Task<IActionResult> GetTopTracksAllTime([FromQuery] int limit = 25)
    {
        var accessToken = await GetValidAccessTokenAsync();
        if (string.IsNullOrEmpty(accessToken)) return Unauthorized("Access token is missing or invalid");

        try
        {
            var topTracks = await _spotifyService.GetTopTracksAsync(accessToken, "long_term", limit);
            return Ok(topTracks);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogError("Unauthorized: {Message}", ex.Message);

            // Clear tokens and force reauthorization
            _tokenService.ClearAccessTokens(); // Make sure this method name exists in TokenService
            return Unauthorized("The access token is invalid or expired");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching top tracks of all time");
            return BadRequest($"Error fetching top tracks of all time: {ex.Message}");
        }
    }

    [HttpGet("top-tracks")]
    public async Task<IActionResult> GetTopTracks([FromQuery] string timeRange = "medium_term")
    {
        var accessToken = await GetValidAccessTokenAsync();
        if (string.IsNullOrEmpty(accessToken)) return Unauthorized("Access token is missing or invalid");

        try
        {
            var tracks = await _spotifyService.GetTopTracksAsync(accessToken, timeRange);
            return Ok(tracks);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogError("Unauthorized: {Message}", ex.Message);

            // Clear tokens and force reauthorization
            _tokenService.ClearAccessTokens();
            return Unauthorized("The access token is invalid or expired");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching top tracks");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("currently-playing")]
    public async Task<IActionResult> GetCurrentlyPlayingTrack()
    {
        var accessToken = await GetValidAccessTokenAsync();
        if (string.IsNullOrEmpty(accessToken)) return Unauthorized("Access token is missing or invalid");

        try
        {
            var currentlyPlaying = await _spotifyService.GetCurrentlyPlayingTrackAsync(accessToken);
            if (currentlyPlaying == null)
            {
                return Ok(new { Message = "No track is currently playing" });
            }

            return Ok(currentlyPlaying);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogError("Unauthorized: {Message}", ex.Message);
            _tokenService.ClearAccessTokens();
            return Unauthorized("The access token is invalid or expired");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching currently playing track");
            return BadRequest($"Error fetching currently playing track: {ex.Message}");
        }
    }

    [HttpGet("login")]
    public IActionResult Login(string returnUrl = null, string state = null)
    {
        _logger.LogInformation("Login request received with returnUrl: {ReturnUrl} and state length: {StateLength}",
            returnUrl, state?.Length ?? 0);

        var clientId = _configuration["Spotify:ClientId"];

        // This should point to your API's callback
        var redirectUri = $"{Request.Scheme}://{Request.Host}/api/auth/callback";

        // Use the provided state directly - don't generate a new one
        // This will maintain the state value the client sent
        string finalState = state ?? Guid.NewGuid().ToString();

        _logger.LogInformation("Using state: {State}", finalState);

        var scopes = "user-read-private user-read-email user-top-read user-read-currently-playing";

        var authorizationUrl = $"https://accounts.spotify.com/authorize" +
                              $"?client_id={Uri.EscapeDataString(clientId)}" +
                              $"&response_type=code" +
                              $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                              $"&scope={Uri.EscapeDataString(scopes)}" +
                              $"&state={Uri.EscapeDataString(finalState)}";

        _logger.LogInformation("Redirecting to Spotify authorization: {Url}", authorizationUrl);
        return Redirect(authorizationUrl);
    }



    [HttpGet("logout")]
    public IActionResult Logout(string returnUrl = null)
    {
        _logger.LogInformation("Logout request received");

        // Default redirect to client app if not specified
        if (string.IsNullOrEmpty(returnUrl))
        {
            returnUrl = _clientAppUrl;
        }
        else if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Absolute) && !returnUrl.StartsWith("/"))
        {
            returnUrl = _clientAppUrl;
        }

        _tokenService.ClearAccessTokens();
        _logger.LogInformation("User signed out successfully, redirecting to {ReturnUrl}", returnUrl);

        return Redirect(returnUrl);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback(string code, string state)
    {
        _logger.LogInformation("Spotify OAuth callback received with code length: {CodeLength}, state length: {StateLength}",
            code?.Length ?? 0, state?.Length ?? 0);

        if (string.IsNullOrEmpty(code))
        {
            _logger.LogError("Authorization code is missing.");
            return Redirect($"{_clientAppUrl}?error=missing_code");
        }

        var clientId = _configuration["Spotify:ClientId"];
        var clientSecret = _configuration["Spotify:ClientSecret"];
        var redirectUri = $"{Request.Scheme}://{Request.Host}/api/auth/callback";

        // Exchange the authorization code for an access token
        var tokenResponse = await _spotifyService.ExchangeCodeForTokenAsync(code, clientId, clientSecret, redirectUri);

        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
        {
            _logger.LogError("Failed to exchange authorization code for token.");
            return Redirect($"{_clientAppUrl}?error=token_exchange_failed");
        }

        // Store the access token and refresh token
        _tokenService.SetAccessToken(tokenResponse.AccessToken, tokenResponse.ExpiresIn);
        _tokenService.SetRefreshToken(tokenResponse.RefreshToken);

        // For client-side auth, also append the token to the redirect URL as a fragment
        var redirectUrl = $"{_clientAppUrl}/spotify-callback?code={Uri.EscapeDataString(code)}";

        // Include the original state if available
        if (!string.IsNullOrEmpty(state))
        {
            redirectUrl += $"&state={Uri.EscapeDataString(state)}";
        }

        _logger.LogInformation("Successfully exchanged code for token. Redirecting to {RedirectUrl}", redirectUrl);
        return Redirect(redirectUrl);
    }



    [HttpGet("handle-state-error")]
    public async Task<IActionResult> HandleStateError(string code, string state = null)
    {
        _logger.LogInformation("Handling Spotify OAuth callback with code: {CodeStart}..., state: {StateStart}...",
            code?.Substring(0, Math.Min(10, code?.Length ?? 0)),
            state?.Substring(0, Math.Min(10, state?.Length ?? 0)));

        try
        {
            if (string.IsNullOrEmpty(code))
            {
                _logger.LogError("No authorization code found in request");
                return BadRequest(new { error = "code_required" });
            }

            var clientId = _configuration["Spotify:ClientId"];
            var clientSecret = _configuration["Spotify:ClientSecret"];
            var redirectUri = $"{Request.Scheme}://{Request.Host}/api/auth/handle-state-error";

            // Use SpotifyService to exchange the authorization code for an access token
            var tokenResponse = await _spotifyService.ExchangeCodeForTokenAsync(code, clientId, clientSecret, redirectUri);

            if (tokenResponse == null)
            {
                _logger.LogError("Token response is null after exchange attempt");
                return Redirect($"{_clientAppUrl}/?error=token_exchange_failed");
            }

            if (string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                _logger.LogError("Access token is empty or null in token response");
                _logger.LogInformation("Token response properties: TokenType={TokenType}, Scope={Scope}, ExpiresIn={ExpiresIn}, RefreshToken={HasRefreshToken}",
                    tokenResponse.TokenType,
                    tokenResponse.Scope,
                    tokenResponse.ExpiresIn,
                    !string.IsNullOrEmpty(tokenResponse.RefreshToken));
                return Redirect($"{_clientAppUrl}/?error=token_exchange_failed");
            }

            // Store the access token and refresh token
            _tokenService.SetAccessToken(tokenResponse.AccessToken, tokenResponse.ExpiresIn);

            if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                _tokenService.SetRefreshToken(tokenResponse.RefreshToken);
            }

            _logger.LogInformation("Successfully exchanged code for token. Redirecting to {RedirectUrl}",
                $"{_clientAppUrl}/dashboard");

            // Include original state when redirecting
            var redirectUrl = $"{_clientAppUrl}/dashboard";
            if (!string.IsNullOrEmpty(state))
            {
                redirectUrl += $"?state={Uri.EscapeDataString(state)}";
            }

            return Redirect(redirectUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Spotify OAuth callback");
            return Redirect($"{_clientAppUrl}/?error={Uri.EscapeDataString(ex.Message)}");
        }
    }

    [HttpGet("check-tokens")]
    public IActionResult CheckTokens()
    {
        var accessToken = _tokenService.GetAccessToken();
        var refreshToken = _tokenService.GetRefreshToken();

        return Ok(new
        {
            hasAccessToken = !string.IsNullOrEmpty(accessToken),
            accessTokenLength = accessToken?.Length ?? 0,
            accessTokenExpired = accessToken != null && _tokenService.NeedsRefresh(),
            hasRefreshToken = !string.IsNullOrEmpty(refreshToken),
            refreshTokenLength = refreshToken?.Length ?? 0
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromQuery] string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogError("Refresh token is missing in request");
            return BadRequest(new { error = "refresh_token_required" });
        }

        try
        {
            var clientId = _configuration["Spotify:ClientId"];
            var clientSecret = _configuration["Spotify:ClientSecret"];

            var tokenResponse = await _spotifyService.RefreshAccessTokenAsync(refreshToken, clientId, clientSecret);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                _logger.LogError("Failed to refresh access token");
                return BadRequest(new { error = "token_refresh_failed" });
            }

            _tokenService.SetAccessToken(tokenResponse.AccessToken, tokenResponse.ExpiresIn);

            if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                _tokenService.SetRefreshToken(tokenResponse.RefreshToken);
            }

            _logger.LogInformation("Successfully refreshed access token");

            // Return just what the client needs, don't expose full token details
            return Ok(new
            {
                accessToken = tokenResponse.AccessToken,
                expiresIn = tokenResponse.ExpiresIn,
                hasRefreshToken = !string.IsNullOrEmpty(tokenResponse.RefreshToken)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing access token");
            return StatusCode(500, new { error = "unexpected_error" });
        }
    }
}
