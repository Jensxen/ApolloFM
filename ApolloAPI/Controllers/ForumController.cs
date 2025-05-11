// ApolloAPI/Controllers/ForumController.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FM.Application.Services.ForumServices;
using FM.Application.Services.ServiceDTO;
using Microsoft.AspNetCore.Mvc;

namespace ApolloAPI.Controllers
{
    [ApiController]
    [Route("api/forum")]
    public class ForumController : ControllerBase
    {
        private readonly IForumService _forumService;

        public ForumController(IForumService forumService)
        {
            _forumService = forumService;
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

                // Get user ID from claims (implement proper auth!)
                var userId = User.Identity.IsAuthenticated ? User.Identity.Name : "anonymous";

                var topicDto = await _forumService.CreateTopicAsync(createTopicDto, userId);
                return CreatedAtAction(nameof(GetTopic), new { id = topicDto.Id }, topicDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
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

                // Get user ID from claims (implement proper auth!)
                var userId = User.Identity.IsAuthenticated ? User.Identity.Name : "anonymous";

                var commentDto = await _forumService.AddCommentAsync(addCommentDto, userId);
                return CreatedAtAction(nameof(GetTopic), new { id = addCommentDto.PostId }, commentDto);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
