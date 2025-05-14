// ApolloAPI/Controllers/ForumController.cs
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FM.Application.Services.ForumServices;
using FM.Application.Services.ServiceDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ApolloAPI.Controllers
{
    [ApiController]
    [Route("api/forum")]
    public class ForumController : ControllerBase
    {
        private readonly IForumService _forumService;
        private readonly ILogger<ForumController> _logger;

        public ForumController(IForumService forumService, ILogger<ForumController> logger)
        {
            _forumService = forumService;
            _logger = logger;
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

                // Get user ID from claims properly and set it in the DTO
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID could not be determined from authentication token");
                }

                _logger.LogInformation("Creating topic with user ID: {UserId}", userId);

                // Set the user ID in the DTO
                createTopicDto.UserId = userId;

                // Call the service method with just the DTO
                var topicDto = await _forumService.CreateTopicAsync(createTopicDto);
                return CreatedAtAction(nameof(GetTopic), new { id = topicDto.Id }, topicDto);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
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

                // Get user ID from claims properly
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID could not be determined from authentication token");
                }

                _logger.LogInformation("Adding comment with user ID: {UserId}", userId);

                var commentDto = await _forumService.AddCommentAsync(addCommentDto, userId);
                return CreatedAtAction(nameof(GetTopic), new { id = addCommentDto.PostId }, commentDto);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the current user ID from claims
        /// </summary>
        private string GetCurrentUserId()
        {
            // First try to get the ID from the Name claim (common with JWT tokens)
            var userId = User.Identity?.Name;

            // If not found, try to get from NameIdentifier claim (common with many auth providers)
            if (string.IsNullOrEmpty(userId))
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            // If still not found, try with a custom claim that might contain the Spotify ID
            if (string.IsNullOrEmpty(userId))
            {
                userId = User.FindFirstValue("spotify_id");
            }

            // Log which claim provided the user ID for debugging
            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogDebug("User ID {UserId} found in claims", userId);
            }
            else
            {
                _logger.LogWarning("No user ID found in claims");
            }

            return userId;
        }
    }
}
