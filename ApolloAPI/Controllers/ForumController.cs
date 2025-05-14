// ApolloAPI/Controllers/ForumController.cs
using FM.Application.Interfaces.IRepositories;
using FM.Application.Services.ForumServices;
using FM.Application.Services.ServiceDTO;
using FM.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
namespace ApolloAPI.Controllers
{
    [ApiController]
    [Route("api/forum")]
    public class ForumController : ControllerBase
    {
        private readonly IForumService _forumService;
        private readonly ILogger<ForumController> _logger;
        private readonly IUserRepository _userRepository;
        private readonly TokenService _tokenService;

        public ForumController(IForumService forumService, ILogger<ForumController> logger, IUserRepository userRepository, TokenService tokenService)
        {
            _forumService = forumService;
            _logger = logger;
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        [HttpGet("topics")]
        public async Task<ActionResult<List<ForumTopicDto>>> GetTopics()
        {
            try
            {
                var topics = await _forumService.GetTopicsAsync();
                return Ok(topics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting forum topics");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("topics/{id}")]
        public async Task<ActionResult<ForumTopicDto>> GetTopic(int id)
        {
            try
            {
                var topic = await _forumService.GetTopicByIdAsync(id);

                if (topic == null)
                {
                    return NotFound($"Topic with ID {id} not found");
                }

                return Ok(topic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting forum topic {TopicId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("subforums")]
        public async Task<ActionResult<List<SubForumDto>>> GetSubForums()
        {
            try
            {
                var subforums = await _forumService.GetSubForumsAsync();
                return Ok(subforums);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subforums");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("topics")]
        public async Task<ActionResult<ForumTopicDto>> CreateTopic([FromBody] CreateTopicDto createTopicDto)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(createTopicDto.Title) || string.IsNullOrEmpty(createTopicDto.Content))
                {
                    return BadRequest("Title and content are required.");
                }

                // Get and verify user identity
                var userId = await GetUserIdFromTokenServiceOrRequest();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("No user ID could be found. Creating topic as anonymous.");
                    userId = "119830768"; // Use known working ID for anonymous posts
                }

                var topicDto = await _forumService.CreateTopicWithUserIdAsync(createTopicDto, userId);
                return CreatedAtAction(nameof(GetTopic), new { id = topicDto.Id }, topicDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating forum topic");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("comments")]
        public async Task<ActionResult<CommentDto>> AddComment([FromBody] AddCommentDto addCommentDto)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(addCommentDto.Content))
                {
                    return BadRequest("Comment content is required.");
                }

                // Get and verify user identity
                var userId = await GetUserIdFromTokenServiceOrRequest();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("No user ID could be found. Creating comment as anonymous.");
                    userId = "119830768"; // Use known working ID for anonymous comments
                }

                var commentDto = await _forumService.AddCommentAsync(addCommentDto, userId);
                return CreatedAtAction(nameof(GetTopic), new { id = addCommentDto.PostId }, commentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        private async Task<string> GetUserIdFromTokenServiceOrRequest()
        {
            // Try TokenService first
            var token = _tokenService.GetAccessToken();
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var spotifyUser = await GetSpotifyUserProfile(token);
                    if (spotifyUser != null)
                    {
                        // Find or create user by Spotify ID
                        var user = await _userRepository.GetBySpotifyIdAsync(spotifyUser.Id);

                        // Update user if needed
                        if (user != null)
                        {
                            // Update display name if changed
                            if (user.DisplayName != spotifyUser.DisplayName && !string.IsNullOrEmpty(spotifyUser.DisplayName))
                            {
                                user.UpdateDisplayName(spotifyUser.DisplayName);
                                await _userRepository.UpdateUserAsync(user);
                            }

                            return user.Id;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting user profile from Spotify token");
                }
            }

            // Try Authorization header if TokenService didn't work
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var requestToken = authHeader.Substring("Bearer ".Length).Trim();

                try
                {
                    var spotifyUser = await GetSpotifyUserProfile(requestToken);
                    if (spotifyUser != null)
                    {
                        var user = await _userRepository.GetBySpotifyIdAsync(spotifyUser.Id);
                        if (user != null)
                        {
                            return user.Id;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting user profile from request token");
                }
            }

            // If we get here, we couldn't identify the user
            return null;
        }

        private async Task<SpotifyAPI.Web.PrivateUser> GetSpotifyUserProfile(string accessToken)
        {
            try
            {
                var config = SpotifyAPI.Web.SpotifyClientConfig.CreateDefault().WithToken(accessToken);
                var spotify = new SpotifyAPI.Web.SpotifyClient(config);
                return await spotify.UserProfile.Current();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user profile from Spotify");
                return null;
            }
        }
    }
}