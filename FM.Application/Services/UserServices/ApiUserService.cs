// Update your ApiUserService to handle Blazor WebAssembly authentication
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace FM.Application.Services.UserServices
{
    public class ApiUserService : IUserService
    {
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ApiUserService> _logger;

        public ApiUserService(
            AuthenticationStateProvider authStateProvider,
            IHttpClientFactory httpClientFactory,
            ILogger<ApiUserService> logger)
        {
            _authStateProvider = authStateProvider;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<string> GetCurrentUserIdAsync()
        {
            try
            {
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                if (user.Identity?.IsAuthenticated != true)
                {
                    _logger.LogWarning("User is not authenticated");
                    return null;
                }

                // Try different claim types to find user ID
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                    userId = user.FindFirst("sub")?.Value;
                
                if (string.IsNullOrEmpty(userId))
                    userId = user.FindFirst("spotify_id")?.Value;
                
                if (string.IsNullOrEmpty(userId))
                    userId = user.Identity.Name;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("No user ID found in claims. Available claims: {Claims}", 
                        string.Join(", ", user.Claims.Select(c => $"{c.Type}: {c.Value}")));
                }
                else
                {
                    _logger.LogInformation("Found user ID: {UserId}", userId);
                }

                return userId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user ID");
                return null;
            }
        }

        public string GetCurrentUserId()
        {
            return GetCurrentUserIdAsync().GetAwaiter().GetResult();
        }
    }
}