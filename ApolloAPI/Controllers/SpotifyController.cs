using AspNet.Security.OAuth.Spotify;
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

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    [HttpGet("login")]
    public IActionResult Login(string returnUrl = "/")
    {
        _logger.LogInformation("Login attempt with return URL: {ReturnUrl}", returnUrl);
        
        // Validate the return URL (to prevent open redirect attacks)
        if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Absolute) &&
            !returnUrl.StartsWith("/"))
        {
            _logger.LogWarning("Invalid return URL, defaulting to /");
            returnUrl = "/";
        }

        // Generate a random state parameter to prevent CSRF
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
        
        // This method will be called by Spotify after user authentication
        try
        {
            // Get the authentication result from Spotify
            var result = await HttpContext.AuthenticateAsync(SpotifyAuthenticationDefaults.AuthenticationScheme);
            
            if (!result.Succeeded)
            {
                _logger.LogError("Authentication failed: {Failure}", result.Failure?.Message);
                return Redirect("/?error=authentication_failed");
            }
            
            // Get the return URL from the authentication properties
            var returnUrl = result.Properties?.Items["returnUrl"] ?? "/";
            _logger.LogInformation("Authentication successful, redirecting to {ReturnUrl}", returnUrl);
            
            // Complete the authentication by signing in
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                result.Principal,
                new AuthenticationProperties 
                { 
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
                }
            );
            
            // Redirect back to the client application
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
        
        // Validate the return URL (to prevent open redirect attacks)
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
            // Use the token to get user info from Spotify
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
