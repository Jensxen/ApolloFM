using Microsoft.AspNetCore.Mvc;
using FM.Application.Interfaces.ICommand;
using FM.Application.Command.CommandDTO.UserCommandDTO;
using FM.Application.QueryDTO;
using FM.Domain.Entities;
using FM.Application.Interfaces.IRepositories;
using FM.Application.QueryDTO.UserDTO;

namespace ApolloAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserCommand _userCommand;

        public UserController(IUserRepository userRepository, IUserCommand userCommand)
        {
            _userRepository = userRepository;
            _userCommand = userCommand;
        }

        [HttpGet]
        public async Task<IEnumerable<UserQueryDTO>> Get()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return users.Select(u => new UserQueryDTO
            {
                Id = u.Id,
                DisplayName = u.DisplayName,
                SpotifyUserId = u.SpotifyUserId,
                UserRoleId = u.UserRoleId
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserQueryDTO>> Get(string id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return new UserQueryDTO
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                SpotifyUserId = user.SpotifyUserId,
                UserRoleId = user.UserRoleId
            };
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CreateUserCommandDTO command)
        {
            _userCommand.CreateUser(command);
            var createdUser = await _userRepository.GetUserByIdAsync(command.SpotifyUserId); // Assuming SpotifyUserId is unique
            return CreatedAtAction(nameof(Get), new { id = createdUser.Id }, new UserQueryDTO
            {
                Id = createdUser.Id,
                DisplayName = createdUser.DisplayName,
                SpotifyUserId = createdUser.SpotifyUserId,
                UserRoleId = createdUser.UserRoleId
            });
        }

        [HttpPut("{id}")]
        public ActionResult Put(string id, [FromBody] UpdateUserCommandDTO command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            _userCommand.UpdateUser(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(string id, [FromBody] DeleteUserCommandDTO command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            _userCommand.DeleteUser(command);
            return NoContent();
        }
    }
}
