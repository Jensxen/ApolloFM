using Microsoft.AspNetCore.Mvc;
using FM.Domain.Entities;
using FM.Application.Interfaces;

namespace ApolloAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserRepository userRepository, ILogger<UserController> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IEnumerable<User>> Get()
    {
        return await _userRepository.GetAllUsersAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> Get(string id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return user;
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] User user)
    {
        await _userRepository.AddUserAsync(user);
        return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Put(string id, [FromBody] User user)
    {
        if (id != user.Id)
        {
            return BadRequest();
        }

        await _userRepository.UpdateUserAsync(user);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        await _userRepository.DeleteUserAsync(id);
        return NoContent();
    }
}
