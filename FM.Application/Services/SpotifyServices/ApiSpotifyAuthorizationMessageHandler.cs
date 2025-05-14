using FM.Application.Services.AuthServices;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FM.Application.Services.SpotifyServices
{
    /// <summary>
    /// Server-side version of SpotifyAuthorizationMessageHandler that doesn't depend on
    /// Blazor's NavigationManager
    /// </summary>
    public class ApiSpotifyAuthorizationMessageHandler : DelegatingHandler
    {
        private readonly TokenService _tokenService;
        private readonly ILogger<ApiSpotifyAuthorizationMessageHandler> _logger;

        public ApiSpotifyAuthorizationMessageHandler(
            TokenService tokenService, 
            ILogger<ApiSpotifyAuthorizationMessageHandler> logger)
        {
            _tokenService = tokenService;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Get the access token
            var token = _tokenService.GetAccessToken();

            // Check if token is valid or needs refresh
            if (string.IsNullOrEmpty(token) || _tokenService.NeedsRefresh())
            {
                // Try to use refresh token if available
                var refreshToken = _tokenService.GetRefreshToken();
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    try
                    {
                        using var httpClient = new HttpClient();
                        httpClient.BaseAddress = request.RequestUri != null 
                            ? new Uri($"{request.RequestUri.Scheme}://{request.RequestUri.Authority}")
                            : new Uri("https://localhost:7043/");

                        // Call API to refresh token
                        var response = await httpClient.PostAsync(
                            $"api/auth/refresh?refreshToken={Uri.EscapeDataString(refreshToken)}",
                            null, cancellationToken);

                        if (response.IsSuccessStatusCode)
                        {
                            var tokenResponse = await response.Content.ReadFromJsonAsync<AuthService.TokenResponse>(cancellationToken: cancellationToken);
                            if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.AccessToken))
                            {
                                _tokenService.SetAccessToken(tokenResponse.AccessToken, tokenResponse.ExpiresIn);

                                if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
                                {
                                    _tokenService.SetRefreshToken(tokenResponse.RefreshToken);
                                }

                                token = tokenResponse.AccessToken;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error refreshing token in API message handler");
                    }
                }
            }

            // If we have a token, add the Authorization header
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogDebug("Added authorization token to request: {RequestUri}", request.RequestUri);
            }
            else
            {
                _logger.LogWarning("No token available for API request to {RequestUri}", request.RequestUri);
            }

            // Send the request
            return await base.SendAsync(request, cancellationToken);
        }
    }
}