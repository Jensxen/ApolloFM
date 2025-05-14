// FM.Application/Services/ServiceDTO/UserDTO/HttpUserContext.cs
using FM.Application.Services.UserServices;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FM.Application.Services.ServiceDTO.UserDTO
{
    /// <summary>
    /// Implementation of IUserContext that uses HttpContext for server-side API scenarios
    /// </summary>
    public class ApiUserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            // Try multiple claim types to find a user identifier
            var userId = user.Identity?.Name;

            if (string.IsNullOrEmpty(userId))
            {
                // Use Claims.FirstOrDefault instead of FindFirstValue
                var claim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                userId = claim?.Value;
            }

            if (string.IsNullOrEmpty(userId))
            {
                // Use Claims.FirstOrDefault instead of FindFirstValue
                var claim = user.Claims.FirstOrDefault(c => c.Type == "spotify_id");
                userId = claim?.Value;
            }

            return userId;
        }

        public Task<string> GetCurrentUserIdAsync()
        {
            return Task.FromResult(GetCurrentUserId());
        }
    }
}
