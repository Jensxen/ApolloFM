using AspNet.Security.OAuth.Spotify;
using FM.Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;
using System.Security.Cryptography;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly SpotifyService _spotifyService;

    public AuthController(ILogger<AuthController> logger, SpotifyService spotifyService)
    {
        _logger = logger;
        _spotifyService = spotifyService;
    }

    [HttpGet("top-tracks/all-time")]
    public async Task<IActionResult> GetTopTracksAllTime([FromQuery] int limit = 25)
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");
        if (string.IsNullOrEmpty(accessToken)) return Unauthorized();

        try
        {
            // Call SpotifyService with "long_term" time range
            var topTracks = await _spotifyService.GetTopTracksAsync(accessToken, "long_term", limit);
            return Ok(topTracks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching top tracks of all time");
            return BadRequest($"Error fetching top tracks of all time: {ex.Message}");
        }
    }


    [HttpGet("top-tracks")]
    public async Task<IActionResult> GetTopTracks([FromQuery] string timeRange = "short_term", [FromQuery] int limit = 25)
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");
        if (string.IsNullOrEmpty(accessToken)) return Unauthorized();

        try
        {
            var topTracks = await _spotifyService.GetTopTracksAsync(accessToken, timeRange, limit);
            return Ok(topTracks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching top tracks");
            return BadRequest($"Error fetching top tracks: {ex.Message}");
        }
    }

    [HttpGet("currently-playing")]
    public async Task<IActionResult> GetCurrentlyPlayingTrack()
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");
        if (string.IsNullOrEmpty(accessToken)) return Unauthorized();

        try
        {
            var currentlyPlaying = await _spotifyService.GetCurrentlyPlayingTrackAsync(accessToken);
            if (currentlyPlaying == null)
            {
                return Ok(new { Message = "No song is currently playing." });
            }

            return Ok(currentlyPlaying);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching currently playing track");
            return BadRequest($"Error fetching currently playing track: {ex.Message}");
        }
    }

    [HttpGet("login")]
    public IActionResult Login(string returnUrl = "/")
    {
        _logger.LogInformation("Login attempt with return URL: {ReturnUrl}", returnUrl);

        // Open Redirct Attacks
        if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Absolute) &&
            !returnUrl.StartsWith("/"))
        {
            _logger.LogWarning("Invalid return URL, defaulting to /");
            returnUrl = "/";
        }

        //CSRF
        var state = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        return Challenge(new AuthenticationProperties
        {
            RedirectUri = returnUrl,
            Items =
            {
                { "returnUrl", returnUrl },
                { ".xsrf", state }
            }
        }, SpotifyAuthenticationDefaults.AuthenticationScheme);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback()
    {
        _logger.LogInformation("Authentication callback received");

        try
        {
            var result = await HttpContext.AuthenticateAsync(SpotifyAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                _logger.LogError("Authentication failed: {Failure}", result.Failure?.Message);
                return Redirect("/?error=authentication_failed");
            }

            var returnUrl = result.Properties?.Items["returnUrl"] ?? "/";
            _logger.LogInformation("Authentication successful, redirecting to {ReturnUrl}", returnUrl);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                result.Principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
                }
            );

            return Redirect(returnUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in authentication callback");
            return Redirect("/?error=unexpected_error");
        }
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout(string returnUrl = "/")
    {
        _logger.LogInformation("Logout request received");

        if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Absolute) &&
            !returnUrl.StartsWith("/"))
        {
            returnUrl = "/";
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("User signed out successfully");

        return Redirect(returnUrl);
    }

    [HttpGet("user")]
    public async Task<IActionResult> GetUser()
    {
        if (!User.Identity.IsAuthenticated)
        {
            _logger.LogWarning("Unauthenticated user tried to access user info");
            return Unauthorized();
        }

        var token = await HttpContext.GetTokenAsync("access_token");
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("No access token found for authenticated user");
            return Unauthorized();
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user data from Spotify");
            return BadRequest($"Error fetching user data: {ex.Message}");
        }
    }
}
