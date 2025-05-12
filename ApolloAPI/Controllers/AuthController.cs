using AspNet.Security.OAuth.Spotify;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;
using System.Text;
using FM.Application.Services.SpotifyServices;
using FM.Domain.Entities;
using FM.Application.Interfaces.IRepositories;
using FM.Application.Interfaces;
using FM.Application.Services.ServiceDTO.UserDTO;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly SpotifyService _spotifyService;
    private readonly IConfiguration _configuration;
    private readonly TokenService _tokenService;
    private readonly string _clientAppUrl;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    private string GenerateAndStoreState()
    {
        var state = Guid.NewGuid().ToString();

        HttpContext.Session.SetString("OAuthState", state);

        return state;
    }

    public AuthController(
        ILogger<AuthController> logger,
        SpotifyService spotifyService,
        IConfiguration configuration,
        TokenService tokenService,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _spotifyService = spotifyService;
        _configuration = configuration;
        _tokenService = tokenService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
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
                Images = user.Images,
                ProfilePictureUrl = user.Images?.FirstOrDefault()?.Url
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

    [HttpGet("top-artists")]
    public async Task<IActionResult> GetTopArtists([FromQuery] string timeRange = "medium_term", [FromQuery] int limit = 10)
    {
        var accessToken = await GetValidAccessTokenAsync();
        if (string.IsNullOrEmpty(accessToken)) return Unauthorized("Access token is missing or invalid");

        try
        {
            var topArtists = await _spotifyService.GetTopArtistsAsync(accessToken, timeRange, limit);
            return Ok(topArtists);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogError("Unauthorized: {Message}", ex.Message);
            _tokenService.ClearAccessTokens();
            return Unauthorized("The access token is invalid or expired");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching top artists");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("recently-played")]
    public async Task<IActionResult> GetRecentlyPlayed([FromQuery] int limit = 20)
    {
        var accessToken = await GetValidAccessTokenAsync();
        if (string.IsNullOrEmpty(accessToken)) return Unauthorized("Access token is missing or invalid");

        try
        {
            var recentTracks = await _spotifyService.GetRecentlyPlayedAsync(accessToken, limit);
            return Ok(recentTracks);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogError("Unauthorized: {Message}", ex.Message);
            _tokenService.ClearAccessTokens();
            return Unauthorized("The access token is invalid or expired");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching recently played tracks");
            return StatusCode(500, new { error = ex.Message });
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
    public IActionResult Login([FromQuery] string returnUrl = null, [FromQuery] string clientState = null)
    {
        var clientId = _configuration["Spotify:ClientId"];
        var redirectUri = _configuration["Spotify:RedirectUri"];

        // Allow the client to provide the state (from localStorage)
        string state;
        if (string.IsNullOrEmpty(clientState))
        {
            // Generate server-side state as fallback
            state = Guid.NewGuid().ToString();
        }
        else
        {
            // Use client-provided state
            state = clientState;
            _logger.LogInformation("Using client-provided state: {State}", state);
        }

        // Store in session for server-side validation if needed
        HttpContext.Session.SetString("OAuthState", state);

        _logger.LogInformation("Generated OAuth state: {State}", state);

        var queryParams = new Dictionary<string, string?>
    {
        {"response_type", "code"},
        {"client_id", clientId},
        {"scope", "user-read-private user-read-email user-top-read user-read-currently-playing user-read-playback-state user-read-recently-played"},
        {"redirect_uri", redirectUri},
        {"state", state}
    };

        var queryString = QueryString.Create(queryParams);
        var authorizationEndpoint = $"https://accounts.spotify.com/authorize{queryString}";

        return Redirect(authorizationEndpoint);
    }

    // In your AuthController or a UserController
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request)
    {
        if (string.IsNullOrEmpty(request.SpotifyUserId) || string.IsNullOrEmpty(request.DisplayName))
        {
            return BadRequest("SpotifyUserId and DisplayName are required");
        }

        try
        {
            // Begin transaction
            await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

            try
            {
                // Check if user already exists
                var existingUser = await _userRepository.GetBySpotifyIdAsync(request.SpotifyUserId);
                if (existingUser != null)
                {
                    await _unitOfWork.CommitAsync();
                    return Ok(existingUser); // User already registered
                }

                // Create new user
                var newUser = new User(
                    request.SpotifyUserId,
                    request.DisplayName,
                    request.SpotifyUserId,
                    1 // Default user role ID
                );

                await _userRepository.AddUserAsync(newUser);
                await _unitOfWork.CommitAsync();

                return Ok(newUser);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error during manual user registration, transaction rolled back");
                return StatusCode(500, "An error occurred while registering the user");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RegisterUser endpoint");
            return StatusCode(500, "An unexpected error occurred");
        }
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
        var storedState = HttpContext.Session.GetString("OAuthState");
        if (state != storedState)
        {
            _logger.LogWarning("State mismatch: expected {ExpectedState}, received {ReceivedState}", storedState, state);

            GenerateAndStoreState(); // Generate a new state for the next request
            return Redirect($"/api/auth/login");
        }

        if (string.IsNullOrEmpty(code))
        {
            return BadRequest(new { error = "Code is missing from the callback" });
        }
        _logger.LogInformation("Spotify OAuth callback received with code length: {CodeLength}, state length: {StateLength}",
            code?.Length ?? 0, state?.Length ?? 0);

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

        // Get user profile from Spotify and register user
        try
        {
            var userProfile = await GetSpotifyUserProfile(tokenResponse.AccessToken);
            if (userProfile != null)
            {
                await RegisterOrUpdateUser(userProfile);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user after authentication");
            // Continue with the flow even if registration fails
        }

        // For client-side auth, append the token to the redirect URL as a fragment
        var redirectUrl = $"{_clientAppUrl}/spotify-callback?code={Uri.EscapeDataString(code)}";

        // Include the original state if available
        if (!string.IsNullOrEmpty(state))
        {
            redirectUrl += $"&state={Uri.EscapeDataString(state)}";
        }

        _logger.LogInformation("Successfully exchanged code for token. Redirecting to {RedirectUrl}", redirectUrl);
        return Redirect(redirectUrl);
    }

    // Helper method to get user profile from Spotify
    private async Task<SpotifyAPI.Web.PrivateUser> GetSpotifyUserProfile(string accessToken)
    {
        try
        {
            var config = SpotifyClientConfig.CreateDefault().WithToken(accessToken);
            var spotify = new SpotifyClient(config);
            return await spotify.UserProfile.Current();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user profile from Spotify");
            return null;
        }
    }

    // Helper method to register or update user
    private async Task<User> RegisterOrUpdateUser(SpotifyAPI.Web.PrivateUser spotifyUser)
    {
        if (spotifyUser == null)
        {
            _logger.LogWarning("Cannot register null user profile");
            return null;
        }

        try
        {
            // Start a transaction
            await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

            try
            {
                // Check if user already exists
                var existingUser = await _userRepository.GetBySpotifyIdAsync(spotifyUser.Id);

                if (existingUser != null)
                {
                    // Update user details if needed
                    bool needsUpdate = false;

                    if (existingUser.DisplayName != spotifyUser.DisplayName && !string.IsNullOrEmpty(spotifyUser.DisplayName))
                    {
                        existingUser.UpdateDisplayName(spotifyUser.DisplayName);
                        needsUpdate = true;
                    }

                    // Add other fields to update if needed

                    if (needsUpdate)
                    {
                        await _userRepository.UpdateUserAsync(existingUser);
                        await _unitOfWork.CommitAsync();
                        _logger.LogInformation("Updated existing user: {UserId}", existingUser.Id);
                    }

                    return existingUser;
                }

                // Create new user
                var defaultRole = 1; // Assuming 1 is the "User" role ID

                var newUser = new User(
                    spotifyUser.Id,
                    spotifyUser.DisplayName ?? "Spotify User",
                    spotifyUser.Id,
                    defaultRole
                );

                await _userRepository.AddUserAsync(newUser);

                // Commit the transaction
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Registered new user: {UserId}", newUser.Id);
                return newUser;
            }
            catch (Exception ex)
            {
                // If anything goes wrong, rollback the transaction
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error during user registration, transaction rolled back");
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering/updating user in database");
            return null;
        }
    }





    [HttpGet("handle-state-error")]
    [HttpPost("handle-state-error")]
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

            // Exchange code for token regardless of state
            var tokenResponse = await _spotifyService.ExchangeCodeForTokenAsync(code, clientId, clientSecret, redirectUri);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                _logger.LogError("Token response is null or invalid after exchange attempt");
                return Redirect($"{_clientAppUrl}/?error=token_exchange_failed");
            }

            // Store tokens server-side
            _tokenService.SetAccessToken(tokenResponse.AccessToken, tokenResponse.ExpiresIn);
            if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                _tokenService.SetRefreshToken(tokenResponse.RefreshToken);
            }

            // Pass tokens to client for client-side storage
            var redirectUrl = $"{_clientAppUrl}/spotify-callback?code={Uri.EscapeDataString(code)}";
            if (!string.IsNullOrEmpty(state))
            {
                redirectUrl += $"&state={Uri.EscapeDataString(state)}";
            }

            // You could also add the token directly to help the client
            // redirectUrl += $"&access_token={Uri.EscapeDataString(tokenResponse.AccessToken)}&expires_in={tokenResponse.ExpiresIn}";

            _logger.LogInformation("Successfully exchanged code for token. Redirecting to {RedirectUrl}", redirectUrl);
            return Redirect(redirectUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Spotify OAuth callback");
            return Redirect($"{_clientAppUrl}/?error={Uri.EscapeDataString(ex.Message)}");
        }
    }

    [HttpGet("debug-session")]
    public IActionResult DebugSession([FromQuery] string clientState = null)
    {
        var sessionState = HttpContext.Session.GetString("OAuthState");

        return Ok(new
        {
            serverState = sessionState,
            clientState = clientState,
            match = sessionState == clientState,
            timestamp = DateTime.UtcNow,
            sessionId = HttpContext.Session.Id
        });
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

    [HttpGet("refresh-state")]
    public IActionResult RefreshState()
    {
        // Clear any existing state
        HttpContext.Session.Remove("OAuthState");

        // Generate new state
        var newState = GenerateAndStoreState();

        _logger.LogInformation("OAuth state refreshed: {NewState}", newState);

        return Ok(new
        {
            message = "OAuth state refreshed successfully",
            newState = newState
        });
    }
}
