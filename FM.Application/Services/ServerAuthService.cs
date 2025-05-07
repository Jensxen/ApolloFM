// FM.Application/Services/ApiAuthService.cs
using FM.Application.Interfaces.IServices;
using FM.Application.Services.ServiceDTO;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace FM.Application.Services
{
    public class ServerAuthService : IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenService _tokenService;
        private readonly ILogger<ServerAuthService> _logger;
        private readonly string _clientAppUrl;

        public ServerAuthService(
            IHttpClientFactory httpClientFactory,
            ITokenService tokenService,
            ILogger<ServerAuthService> logger,
            Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _tokenService = tokenService;
            _logger = logger;
            _clientAppUrl = configuration["AppSettings:ClientAppUrl"] ?? "https://localhost:7210";
        }

        public async Task<bool> IsUserAuthenticated()
        {
            var token = await _tokenService.GetAccessToken();
            return !string.IsNullOrEmpty(token);
        }

        public async Task<SpotifyUserProfile> GetUserProfile()
        {
            try
            {
                return await GetApiDataAsync<SpotifyUserProfile>("api/auth/user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile");
                return null;
            }
        }

        public async Task<List<SpotifyDataDTO>> GetTopTracksAsync(string timeRange = "medium_term")
        {
            try
            {
                return await GetApiDataAsync<List<SpotifyDataDTO>>($"api/auth/top-tracks?timeRange={timeRange}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top tracks");
                throw;
            }
        }

        public async Task<SpotifyDataDTO> GetCurrentlyPlayingTrackAsync()
        {
            try
            {
                return await GetApiDataAsync<SpotifyDataDTO>("api/auth/currently-playing");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                // No content means no track is playing
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting currently playing track");
                throw;
            }
        }

        public async Task LogoutAsync()
        {
            await _tokenService.ClearAccessTokens();
            _logger.LogInformation("User logged out");
        }

        public Task LoginAsync()
        {
            // In API context, we don't navigate directly
            _logger.LogInformation("Login method called in API context - this should be handled via HTTP endpoints");
            return Task.CompletedTask;
        }

        public async Task<string> GetValidAccessTokenAsync()
        {
            var token = await _tokenService.GetAccessToken();

            // If token is missing or needs refresh
            if (string.IsNullOrEmpty(token) || await _tokenService.NeedsRefresh())
            {
                var refreshToken = await _tokenService.GetRefreshToken();
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    try
                    {
                        // Call your API to refresh the token
                        var client = _httpClientFactory.CreateClient("ApolloAPI");
                        var response = await client.PostAsync(
                            $"api/auth/refresh?refreshToken={Uri.EscapeDataString(refreshToken)}",
                            null);

                        if (response.IsSuccessStatusCode)
                        {
                            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
                            if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.AccessToken))
                            {
                                await _tokenService.SetAccessToken(tokenResponse.AccessToken, tokenResponse.ExpiresIn);

                                if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
                                {
                                    await _tokenService.SetRefreshToken(tokenResponse.RefreshToken);
                                }

                                token = tokenResponse.AccessToken;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error refreshing token");
                    }
                }
            }

            return token;
        }

        private async Task<T> GetApiDataAsync<T>(string endpoint)
        {
            try
            {
                // Get a valid access token (with automatic refresh if needed)
                var token = await GetValidAccessTokenAsync();

                _logger.LogInformation("Making API request to {Endpoint}", endpoint);

                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No access token available");
                }

                var client = _httpClientFactory.CreateClient("ApolloAPI");

                // Add the authorization header
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API request error on {Endpoint}", endpoint);
                throw;
            }
        }

        public class TokenResponse
        {
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
            public int ExpiresIn { get; set; }
        }
    }
}

