using FM.Application.Services.AuthServices;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FM.Application.Services.SpotifyServices
{
    public class SpotifyAuthorizationMessageHandler : DelegatingHandler
    {
        private readonly TokenService _tokenService;
        private readonly NavigationManager _navigationManager;

        public SpotifyAuthorizationMessageHandler(TokenService tokenService, NavigationManager navigationManager)
        {
            _tokenService = tokenService;
            _navigationManager = navigationManager;
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
                        // Clone the HttpClient factory approach from AuthService for now
                        // In production, this should be refactored to avoid circular dependencies
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
                        Console.Error.WriteLine($"Error refreshing token in message handler: {ex.Message}");
                    }
                }
            }

            // If we have a token, add the Authorization header
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                // No token available - could redirect to login here
                // But instead, we'll let the API return 401 which will be caught by the caller
                Console.WriteLine("No token available for API request");
            }

            // Send the request
            return await base.SendAsync(request, cancellationToken);
        }
    }
}

