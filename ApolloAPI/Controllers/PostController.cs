using Microsoft.AspNetCore.Mvc;
using FM.Application.Interfaces.ICommand;
using FM.Application.Interfaces.IQuery;
using FM.Application.Command.CommandDTO.PostCommandDTO;
using FM.Application.QueryDTO.PostDTO;
using System.Threading.Tasks;

namespace ApolloAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class PostController : ControllerBase
    {
        private readonly IPostQuery _postQuery;
        private readonly IPostCommand _postCommand;

        public PostController(IPostQuery postQuery, IPostCommand postCommand)
        {
            _postQuery = postQuery;
            _postCommand = postCommand;
        }

        [HttpGet("Posts")]
        public async Task<IActionResult> GetAllPosts()
        {
            try
            {
                var posts = await _postQuery.GetAllPostsAsync();
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Posts/{id}")]
        public async Task<IActionResult> GetPostById(int id)
        {
            try
            {
                var post = await _postQuery.GetPostByIdAsync(id);
                if (post == null)
                {
                    return NotFound();
                }
                return Ok(post);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Posts")]
        public async Task<IActionResult> AddPost([FromBody] CreatePostCommandDTO command)
        {
            try
            {
                await _postCommand.CreatePostAsync(command);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("Posts/{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdatePostCommandDTO command)
        {
            try
            {
                command.Id = id;
                await _postCommand.UpdatePostAsync(command);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("Posts/{id}")]
        public async Task<IActionResult> DeletePost(int id, [FromBody] DeletePostCommandDTO command)
        {
            try
            {
                command.Id = id;
                await _postCommand.DeletePostAsync(command);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
