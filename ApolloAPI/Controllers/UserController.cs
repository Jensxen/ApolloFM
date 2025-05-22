using FM.Application.Interfaces.IRepositories;
using FM.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using FM.Infrastructure;
using FM.Application.Interfaces;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ApolloAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UsersController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public UsersController(IUserRepository userRepository, ILogger<UsersController> logger, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        [HttpGet("first")]
        public async Task<ActionResult<User>> GetFirstUser()
        {
            try
            {
                var user = await _userRepository.GetFirstUserAsync();
                if (user == null)
                    return NotFound("No users found in the database");

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving first user");
                return StatusCode(500, "An error occurred while retrieving the first user");
            }
        }

        [HttpGet("current")]
        public async Task<ActionResult<User>> GetCurrentUser()
        {
            // Extract the bearer token from the Authorization header
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                _logger.LogWarning("No valid authorization header found");
                return Unauthorized("No valid authorization token found");
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            try
            {
                // Get the user's Spotify ID from the token
                string spotifyUserId = await GetSpotifyUserIdFromToken(token);

                if (string.IsNullOrEmpty(spotifyUserId))
                {
                    _logger.LogWarning("Could not extract Spotify user ID from token");
                    return Unauthorized("Invalid token or could not retrieve Spotify user ID");
                }

                // Find the user by their Spotify ID
                var user = await _userRepository.GetBySpotifyIdAsync(spotifyUserId);

                if (user == null)
                {
                    _logger.LogWarning("No user found with Spotify ID: {SpotifyUserId}", spotifyUserId);
                    return NotFound($"No user found with Spotify ID: {spotifyUserId}");
                }

                _logger.LogInformation("Found user {UserId} for Spotify ID {SpotifyUserId}", user.Id, spotifyUserId);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user");
                return StatusCode(500, "An error occurred while retrieving the current user");
            }
        }

        private async Task<string> GetSpotifyUserIdFromToken(string token)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.GetAsync("https://api.spotify.com/v1/me");

                if (response.IsSuccessStatusCode)
                {
                    var userInfo = await response.Content.ReadFromJsonAsync<SpotifyUserInfo>();
                    return userInfo?.id;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Spotify API");
                return null;
            }
        }

        private class SpotifyUserInfo
        {
            public string id { get; set; }
            public string display_name { get; set; }
        }

        [HttpPost("convert-to-numeric")]
        public async Task<ActionResult> ConvertToNumericId([FromQuery] string userId)
        {
            try
            {
                // Find the user by ID
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return NotFound($"User with ID {userId} not found");

                // Generate a new numeric ID
                var newNumericId = DateTime.UtcNow.Ticks.ToString().Substring(10);

                // Create a new user with the same data but new ID
                var updatedUser = new User(
                    newNumericId,
                    user.DisplayName,
                    user.SpotifyUserId,
                    user.UserRoleId
                );

                // Start transaction
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Add new user
                    await _userRepository.AddUserAsync(updatedUser);

                    // Delete old user
                    await _userRepository.DeleteUserAsync(userId, user.RowVersion);

                    await _unitOfWork.CommitAsync();

                    return Ok(new { Message = $"User ID updated from {userId} to {newNumericId}", NewId = newNumericId });
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync();
                    _logger.LogError(ex, "Failed to update user ID");
                    return StatusCode(500, "Failed to update user ID");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user ID");
                return StatusCode(500, "An error occurred");
            }
        }
    }
}