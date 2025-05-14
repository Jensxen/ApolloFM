using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;

namespace FM.Application.Services.UserServices
{
    public class ServerUserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ServerUserService> _logger;
        private const string DEFAULT_USER_ID = "spotify_authenticated_user";

        public ServerUserService(
            IHttpContextAccessor httpContextAccessor,
            ILogger<ServerUserService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<string> GetCurrentUserIdAsync()
        {
            // Check if there's a valid token in the request headers
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                _logger.LogDebug("Found bearer token in request headers");
                
                // Verify the token is valid by calling Spotify API
                if (await IsTokenValidAsync(token))
                {
                    _logger.LogDebug("Token is valid - returning default user ID");
                    return DEFAULT_USER_ID; // Return a default ID for any authenticated user
                }
            }
            
            // If we have claims-based authentication, try to use it
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                _logger.LogDebug("User is authenticated through claims");
                return user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                       user.FindFirst("sub")?.Value ?? 
                       user.FindFirst("spotify_id")?.Value ?? 
                       user.Identity.Name ?? 
                       DEFAULT_USER_ID;
            }

            // For development purposes, return a default ID to bypass authentication
            // Comment out this line in production
            _logger.LogWarning("No authentication found - returning default user ID for development");
            return DEFAULT_USER_ID;
        }

        public string GetCurrentUserId()
        {
            return GetCurrentUserIdAsync().GetAwaiter().GetResult();
        }

        private async Task<bool> IsTokenValidAsync(string token)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            try {
                // Make a simple call to Spotify API to verify token
                var response = await httpClient.GetAsync("https://api.spotify.com/v1/me");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error validating Spotify token");
                return false;
            }
        }
    }
}