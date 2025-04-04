using Microsoft.AspNetCore.Mvc;
using FM.Application.Interfaces.ICommand;
using FM.Application.Interfaces.IQuery;
using FM.Application.Command.CommandDTO.UserCommandDTO;
using FM.Application.QueryDTO.UserDTO;
using System.Threading.Tasks;

namespace ApolloAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class UserController : ControllerBase
    {
        private readonly IUserQuery _userQuery;
        private readonly IUserCommand _userCommand;

        public UserController(IUserQuery userQuery, IUserCommand userCommand)
        {
            _userQuery = userQuery;
            _userCommand = userCommand;
        }

        //[HttpGet("Users")]
        //public async Task<IActionResult> GetAllUsers()
        //{
        //    try
        //    {
        //        var users = await _userQuery.GetAllUsersAsync();
        //        return Ok(users);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        [HttpGet("Users/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                var user = await _userQuery.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Users")]
        public async Task<IActionResult> AddUser([FromBody] CreateUserCommandDTO command)
        {
            try
            {
                await _userCommand.CreateUserAsync(command);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("Users/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserCommandDTO command)
        {
            try
            {
                command.Id = id;
                await _userCommand.UpdateUserAsync(command);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("Users/{id}")]
        public async Task<IActionResult> DeleteUser(string id, [FromBody] DeleteUserCommandDTO command)
        {
            if (id != command.Id)
            {
                return BadRequest("User ID mismatch");
            }

            try
            {
                await _userCommand.DeleteUserAsync(command);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}



