using Microsoft.AspNetCore.Mvc;
using FM.Domain.Entities;
using FM.Domain.Interfaces;

namespace ApolloAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class PostController : ControllerBase
{
    private readonly IPostRepository _postRepository;
    private readonly ILogger<PostController> _logger;

    public PostController(IPostRepository postRepository, ILogger<PostController> logger)
    {
        _postRepository = postRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IEnumerable<Post>> Get()
    {
        return await _postRepository.GetAllPostsAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Post>> Get(int id)
    {
        var post = await _postRepository.GetPostByIdAsync(id);
        if (post == null)
        {
            return NotFound();
        }
        return post;
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] Post post)
    {
        await _postRepository.AddPostAsync(post);
        return CreatedAtAction(nameof(Get), new { id = post.Id }, post);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, [FromBody] Post post)
    {
        if (id != post.Id)
        {
            return BadRequest();
        }

        await _postRepository.UpdatePostAsync(post);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _postRepository.DeletePostAsync(id);
        return NoContent();
    }
}
