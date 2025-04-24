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
using System.Net.Http;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;



[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly SpotifyService _spotifyService;
    private readonly IConfiguration _configuration;


    public AuthController(ILogger<AuthController> logger, SpotifyService spotifyService, IConfiguration configuration)
    {
        _logger = logger;
        _spotifyService = spotifyService;
        _configuration = configuration;
    }

    [HttpGet("top-tracks/all-time")]
    public async Task<IActionResult> GetTopTracksAllTime([FromQuery] int limit = 25)
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");
        if (string.IsNullOrEmpty(accessToken)) return Unauthorized();

        try
        {
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
    public async Task<IActionResult> GetTopTracks([FromQuery] string timeRange = "medium_term")
    {
        try
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (string.IsNullOrEmpty(accessToken)) return Unauthorized();

            var tracks = await _spotifyService.GetTopTracksAsync(accessToken, timeRange);
            return Ok(tracks); // This should return JSON
        }
        catch (Exception ex)
        {
            // Return a proper JSON error response, not HTML
            return StatusCode(500, new { error = ex.Message });
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

        // Validér returnUrl
        if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Absolute) &&
            !returnUrl.StartsWith("/"))
        {
            _logger.LogWarning("Invalid return URL, defaulting to /");
            returnUrl = "/";
        }

        // Kode URL'en så vi kan sende den med Spotify og få den tilbage
        var encodedReturnUrl = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(returnUrl));

        // Inkluder encodedReturnUrl som en query parameter i callback URL
        var callbackUrl = Url.Action("Callback", "Auth", new { returnUrl = encodedReturnUrl }, Request.Scheme, Request.Host.Value);

        _logger.LogInformation("Setting callback URL to: {CallbackUrl}", callbackUrl);

        // Lad ASP.NET Core håndtere CSRF-beskyttelsen
        return Challenge(new AuthenticationProperties
        {
            RedirectUri = callbackUrl
        }, SpotifyAuthenticationDefaults.AuthenticationScheme);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback(string returnUrl)
    {
        _logger.LogInformation("Authentication callback received with encoded returnUrl: {ReturnUrl}", returnUrl);

        try
        {
            // Autentificer brugeren gennem Spotify OAuth provider
            var result = await HttpContext.AuthenticateAsync(SpotifyAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                _logger.LogError("Authentication failed: {Failure}", result.Failure?.Message);
                return Redirect("/?error=authentication_failed");
            }

            string decodedReturnUrl = "/dashboard"; // Default værdi

            try
            {
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    var decodedBytes = WebEncoders.Base64UrlDecode(returnUrl);
                    decodedReturnUrl = Encoding.UTF8.GetString(decodedBytes);
                    _logger.LogInformation("Decoded return URL: {DecodedReturnUrl}", decodedReturnUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decoding return URL");
                // Hvis der er en fejl i dekodningen, brug bare standardværdien
            }

            // Hent access_token fra autentificeringen
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("No access token received from Spotify");
                return Redirect("/?error=no_access_token");
            }

            // Gem brugeren i cookie authentication
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                result.Principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
                }
            );

            _logger.LogInformation("User authenticated successfully. Redirecting to: {ReturnUrl} with token", decodedReturnUrl);

            // Redirect med access_token
            if (Uri.IsWellFormedUriString(decodedReturnUrl, UriKind.Absolute))
            {
                return Redirect($"{decodedReturnUrl}#access_token={Uri.EscapeDataString(accessToken)}");
            }
            else
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var cleanReturnUrl = decodedReturnUrl.TrimStart('/');

                _logger.LogInformation("Redirecting to: {BaseUrl}/authentication/login-callback with token", baseUrl);
                return Redirect($"{baseUrl}/authentication/login-callback?access_token={Uri.EscapeDataString(accessToken)}&returnUrl={Uri.EscapeDataString(cleanReturnUrl)}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in authentication callback");
            return Redirect("/?error=unexpected_error");
        }
    }

    [HttpGet("handle-state-error")]
    public async Task<IActionResult> HandleStateError()
    {
        _logger.LogInformation("Handling OAuth state error - attempting to recover authentication");

        try
        {
            // Log all query parameters for debugging
            foreach (var key in Request.Query.Keys)
            {
                _logger.LogInformation($"Query parameter: {key} = {Request.Query[key]}");
            }

            // Få fat i authorization code fra URL'en
            var code = Request.Query["code"].FirstOrDefault();
            if (string.IsNullOrEmpty(code))
            {
                _logger.LogError("No authorization code found in request");
                return Redirect("/?error=no_auth_code");
            }

            // Få clientId og clientSecret fra konfiguration
            var clientId = _configuration["Spotify:ClientId"];
            var clientSecret = _configuration["Spotify:ClientSecret"];

            _logger.LogInformation($"Using Client ID: {clientId?.Substring(0, 4)}... and Secret: {clientSecret != null}");

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                _logger.LogError("Missing client ID or secret");
                return Redirect("/?error=configuration_error");
            }

            // Manuelt gennemfør token udvekslingen
            var tokenResponse = await ExchangeCodeForTokenAsync(code, clientId, clientSecret);

            if (tokenResponse == null)
            {
                _logger.LogError("Token response is null");
                return Redirect("/?error=null_token_response");
            }

            if (string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                _logger.LogError("Access token is empty in response");
                return Redirect("/?error=empty_token");
            }

            _logger.LogInformation($"Successfully exchanged code for token: {tokenResponse.AccessToken.Substring(0, 10)}...");

            // Redirect til authentication/login-callback på klientsiden med tokenet
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var redirectUrl = $"{baseUrl}/authentication/login-callback?access_token={Uri.EscapeDataString(tokenResponse.AccessToken)}";

            _logger.LogInformation($"Redirecting to: {redirectUrl}");
            return Redirect(redirectUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HandleStateError");
            return Redirect("/?error=state_handling_error");
        }
    }

    // Hjælpemetode til at udveksle kode for token
    private async Task<SpotifyTokenResponse> ExchangeCodeForTokenAsync(string code, string clientId, string clientSecret)
    {
        try
        {
            _logger.LogInformation("Exchanging code for token");

            // Forbered redirect URI - skal matche det, der bruges til login
            var redirectUri = $"{Request.Scheme}://{Request.Host}/api/auth/callback";

            // Opbyg token request
            using var httpClient = new HttpClient();

            // Base64-encode client credentials for Basic Auth
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            // Forbered form content
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", redirectUri }
        });

            _logger.LogInformation($"Sending token request to Spotify with redirect_uri: {redirectUri}");

            // Send anmodning til Spotify
            var response = await httpClient.PostAsync("https://accounts.spotify.com/api/token", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Token exchange failed: {response.StatusCode}, {responseContent}");
                return null;
            }

            // Deserialiser svaret
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var tokenResponse = JsonSerializer.Deserialize<SpotifyTokenResponse>(responseContent, options);

            _logger.LogInformation("Successfully deserialized token response");

            return tokenResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging code for token");
            return null;
        }
    }

    // Hjælpeklasse til token response
    private class SpotifyTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
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
