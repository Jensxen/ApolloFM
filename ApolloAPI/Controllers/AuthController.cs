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
        {
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

    }

    [HttpGet("login")]
    public IActionResult Login(string returnUrl = "/")
    {
        var clientId = _configuration["Spotify:ClientId"];
        var redirectUri = $"{Request.Scheme}://{Request.Host}/api/auth/callback";
        var scopes = "user-read-private user-read-email user-top-read";

        var state = Guid.NewGuid().ToString(); // Generate a random state value
        var authorizationUrl = $"https://accounts.spotify.com/authorize" +
                               $"?client_id={Uri.EscapeDataString(clientId)}" +
                               $"&response_type=code" +
                               $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                               $"&scope={Uri.EscapeDataString(scopes)}" +
                               $"&state={Uri.EscapeDataString(state)}";

        return Redirect(authorizationUrl);
    }


    [HttpGet("callback")]
    public async Task<IActionResult> Callback(string code, string state)
    {
        if (string.IsNullOrEmpty(code))
        {
            _logger.LogError("Authorization code is missing.");
            return Redirect("/?error=missing_code");
        }

        var clientId = _configuration["Spotify:ClientId"];
        var clientSecret = _configuration["Spotify:ClientSecret"];
        var redirectUri = $"{Request.Scheme}://{Request.Host}/api/auth/callback";

        // Exchange the authorization code for an access token
        var tokenResponse = await _spotifyService.ExchangeCodeForTokenAsync(code, clientId, clientSecret, redirectUri);

        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
        {
            _logger.LogError("Failed to exchange authorization code for token.");
            return Redirect("/?error=token_exchange_failed");
        }

        // Store the access token and refresh token
        _tokenService.SetAccessToken(tokenResponse.AccessToken, tokenResponse.ExpiresIn);
        _tokenService.SetRefreshToken(tokenResponse.RefreshToken);

        return Redirect("/dashboard");
    }


    [HttpGet("logout")]
    public async Task<IActionResult> Logout(string returnUrl = "/")
    {
        _logger.LogInformation("Logout request received");

        if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Absolute) && !returnUrl.StartsWith("/"))
        {
            returnUrl = "/";
        }

        _tokenService.ClearAccessTokens();
        _logger.LogInformation("User signed out successfully");

        return Redirect(returnUrl);
    }


    [HttpGet("handle-state-error")]
    public async Task<IActionResult> HandleStateError(string code)
    {
        _logger.LogInformation("Handling Spotify OAuth callback with code: {CodeStart}...",
            code?.Substring(0, Math.Min(10, code?.Length ?? 0)));

        try
        {
            if (string.IsNullOrEmpty(code))
            {
                _logger.LogError("No authorization code found in request");
                return Redirect("/?error=no_auth_code");
            }

            var clientId = _configuration["Spotify:ClientId"];
            var clientSecret = _configuration["Spotify:ClientSecret"];
            var redirectUri = $"{Request.Scheme}://{Request.Host}/api/auth/handle-state-error";

            // Use SpotifyService to exchange the authorization code for an access token
            var tokenResponse = await _spotifyService.ExchangeCodeForTokenAsync(code, clientId, clientSecret, redirectUri);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                _logger.LogError("Failed to exchange code for token");
                return Redirect("/?error=token_exchange_failed");
            }

            // Store the access token and refresh token
            _tokenService.SetAccessToken(tokenResponse.AccessToken, tokenResponse.ExpiresIn);

            if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                _tokenService.SetRefreshToken(tokenResponse.RefreshToken);
            }

            _logger.LogInformation("Successfully exchanged code for token. Redirecting to /dashboard");

            // Redirect the user to the dashboard
            return Redirect("/dashboard");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Spotify OAuth callback");
            return Redirect("/?error=unexpected_error");
        }
    }




    [HttpGet("user")]
    public async Task<IActionResult> GetUser()
    {
        if (!User.Identity.IsAuthenticated)
        {
            _logger.LogWarning("Unauthenticated user tried to access user info");
            return Unauthorized();
        }

        var token = await GetValidAccessTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("No access token found for authenticated user");
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
            _logger.LogError("Unauthorized: {Message}", ex.Message);

            // Clear tokens and force reauthorization
            _tokenService.ClearAccessTokens();
            return Unauthorized("The access token is invalid or expired");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user data from Spotify");
            return BadRequest($"Error fetching user data: {ex.Message}");
        }
    }

}
