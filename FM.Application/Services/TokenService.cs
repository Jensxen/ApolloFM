using System;
using System.Threading;

namespace FM.Application.Services
{
    public class TokenService
    {
        private string _accessToken;
        private string _refreshToken;
        private DateTime _expirationTime;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Retrieves the current access token if it is still valid.
        /// </summary>
        public string GetAccessToken()
        {
            if (string.IsNullOrEmpty(_accessToken) || DateTime.UtcNow >= _expirationTime)
            {
                return null; // Token is either missing or expired
            }

            return _accessToken;
        }

        /// <summary>
        /// Checks if the access token needs to be refreshed.
        /// </summary>
        /// <returns>True if the token is missing, expired, or will expire soon. False otherwise.</returns>
        public bool NeedsRefresh()
        {
            // If we have no token, or if it's expired or will expire soon (within 5 minutes)
            return string.IsNullOrEmpty(_accessToken) || 
                   DateTime.UtcNow.AddMinutes(5) >= _expirationTime;
        }

        /// <summary>
        /// Sets the access token and its expiration time.
        /// </summary>
        public void SetAccessToken(string accessToken, int expiresInSeconds)
        {
            if (string.IsNullOrEmpty(accessToken)) throw new ArgumentNullException(nameof(accessToken));

            _lock.Wait();
            try
            {
                _accessToken = accessToken;
                _expirationTime = DateTime.UtcNow.AddSeconds(expiresInSeconds);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Retrieves the current refresh token.
        /// </summary>
        public string GetRefreshToken()
        {
            return _refreshToken;
        }

        /// <summary>
        /// Sets the refresh token.
        /// </summary>
        public void SetRefreshToken(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken)) throw new ArgumentNullException(nameof(refreshToken));

            _lock.Wait();
            try
            {
                _refreshToken = refreshToken;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Clears the access token, refresh token, and expiration time.
        /// </summary>
        public void ClearAccessToken()
        {
            _lock.Wait();
            try
            {
                _accessToken = null;
                _refreshToken = null;
                _expirationTime = DateTime.MinValue;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Clears all tokens (alias for ClearAccessToken for consistency).
        /// </summary>
        public void ClearAccessTokens()
        {
            ClearAccessToken();
        }
    }
}
