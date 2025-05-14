using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FM.Application.Services.UserServices
{
    public class UserService : IUserService
    {
        private readonly AuthenticationStateProvider _authStateProvider;

        public UserService(AuthenticationStateProvider authStateProvider)
        {
            _authStateProvider = authStateProvider;
        }

        public async Task<string> GetCurrentUserIdAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            return authState.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public string GetCurrentUserId()
        {
            // Synchronous version (less ideal for Blazor WebAssembly)
            return GetCurrentUserIdAsync().GetAwaiter().GetResult();
        }
    }
}
