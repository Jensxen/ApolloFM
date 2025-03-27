using Microsoft.AspNetCore.Mvc;
using FM.Application.Interfaces.ICommand;
using FM.Application.Command.CommandDTO;
using FM.Domain.Entities;
using FM.Application.QueryDTO.SubForumDTO;
using FM.Application.Interfaces.IRepositories;
using FM.Application.Command.CommandDTO.SubForumCommandDTO;

namespace ApolloAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SubForumController : ControllerBase
    {
        private readonly ISubForumRepository _subForumRepository;
        private readonly ISubForumCommand _subForumCommand;

        public SubForumController(ISubForumRepository subForumRepository, ISubForumCommand subForumCommand)
        {
            _subForumRepository = subForumRepository;
            _subForumCommand = subForumCommand;
        }

        [HttpGet]
        public async Task<IEnumerable<SubForumQueryDTO>> Get()
        {
            var subForums = await _subForumRepository.GetAllSubForumsAsync();
            return subForums.Select(sf => new SubForumQueryDTO
            {
                Id = sf.Id,
                Name = sf.Name,
                Description = sf.Description
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SubForumQueryDTO>> Get(int id)
        {
            var subForum = await _subForumRepository.GetSubForumByIdAsync(id);
            if (subForum == null)
            {
                return NotFound();
            }
            return new SubForumQueryDTO
            {
                Id = subForum.Id,
                Name = subForum.Name,
                Description = subForum.Description
            };
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CreateSubForumCommandDTO command)
        {
            _subForumCommand.CreateSubForum(command);
            var createdSubForum = await _subForumRepository.GetAllSubForumsAsync();
            var newSubForum = createdSubForum.FirstOrDefault(sf => sf.Name == command.Name && sf.Description == command.Description);
            if (newSubForum == null)
            {
                return BadRequest("SubForum creation failed.");
            }
            return CreatedAtAction(nameof(Get), new { id = newSubForum.Id }, new SubForumQueryDTO
            {
                Id = newSubForum.Id,
                Name = newSubForum.Name,
                Description = newSubForum.Description
            });
        }

        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody] UpdateSubForumCommandDTO command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            _subForumCommand.UpdateSubForum(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id, [FromBody] DeleteSubForumCommandDTO command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            _subForumCommand.DeleteSubForum(command);
            return NoContent();
        }
    }
}


