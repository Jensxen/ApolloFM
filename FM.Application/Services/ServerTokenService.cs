// FM.Application/Services/ServerTokenService.cs
using FM.Application.Interfaces.IServices;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;

namespace FM.Application.Services
{
    public class ServerTokenService : ITokenService
    {
        private readonly IDistributedCache _cache;
        private const string ACCESS_TOKEN_KEY = "spotify_access_token";
        private const string REFRESH_TOKEN_KEY = "spotify_refresh_token";
        private const string TOKEN_EXPIRY_KEY = "spotify_token_expiry";

        public ServerTokenService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<string> GetAccessToken()
        {
            return await _cache.GetStringAsync(ACCESS_TOKEN_KEY);
        }

        public async Task SetAccessToken(string accessToken, int expiresInSeconds)
        {
            if (string.IsNullOrEmpty(accessToken))
                return;

            await _cache.SetStringAsync(ACCESS_TOKEN_KEY, accessToken);
            
            var expirationTime = DateTime.UtcNow.AddSeconds(expiresInSeconds);
            await _cache.SetStringAsync(TOKEN_EXPIRY_KEY, expirationTime.ToString("o"));
        }

        public async Task<string> GetRefreshToken()
        {
            return await _cache.GetStringAsync(REFRESH_TOKEN_KEY);
        }

        public async Task SetRefreshToken(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return;
                
            await _cache.SetStringAsync(REFRESH_TOKEN_KEY, refreshToken);
        }

        public async Task<bool> NeedsRefresh()
        {
            var expiryString = await _cache.GetStringAsync(TOKEN_EXPIRY_KEY);
            
            if (string.IsNullOrEmpty(expiryString))
                return true;
                
            if (DateTime.TryParse(expiryString, out var expiry))
            {
                return expiry < DateTime.UtcNow.AddMinutes(5);
            }
            
            return true;
        }

        public async Task ClearAccessTokens()
        {
            await _cache.RemoveAsync(ACCESS_TOKEN_KEY);
            await _cache.RemoveAsync(REFRESH_TOKEN_KEY);
            await _cache.RemoveAsync(TOKEN_EXPIRY_KEY);
        }
    }
}
