using Microsoft.AspNetCore.Mvc;
using FM.Application.Interfaces.ICommand;
using FM.Application.Interfaces.IQuery;
using FM.Application.Command.CommandDTO.SubForumCommandDTO;
using FM.Application.QueryDTO.SubForumDTO;
using System.Threading.Tasks;

namespace ApolloAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class SubForumController : ControllerBase
    {
        private readonly ISubForumQuery _subForumQuery;
        private readonly ISubForumCommand _subForumCommand;

        public SubForumController(ISubForumQuery subForumQuery, ISubForumCommand subForumCommand)
        {
            _subForumQuery = subForumQuery;
            _subForumCommand = subForumCommand;
        }

        [HttpGet("SubForums")]
        public async Task<IActionResult> GetAllSubForums()
        {
            try
            {
                var subForums = await _subForumQuery.GetAllSubForumsAsync();
                return Ok(subForums);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("SubForums/{id}")]
        public async Task<IActionResult> GetSubForumById(int id)
        {
            try
            {
                var subForum = await _subForumQuery.GetSubForumByIdAsync(id);
                if (subForum == null)
                {
                    return NotFound();
                }
                return Ok(subForum);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SubForums")]
        public async Task<IActionResult> AddSubForum([FromBody] CreateSubForumCommandDTO command)
        {
            try
            {
                await _subForumCommand.CreateSubForum(command);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("SubForums/{id}")]
        public async Task<IActionResult> UpdateSubForum(int id, [FromBody] UpdateSubForumCommandDTO command)
        {
            try
            {
                command.Id = id;
                await _subForumCommand.UpdateSubForum(command);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("SubForums/{id}")]
        public async Task<IActionResult> DeleteSubForum(int id, [FromBody] DeleteSubForumCommandDTO command)
        {
            try
            {
                command.Id = id;
                await _subForumCommand.DeleteSubForum(command);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

