using Microsoft.AspNetCore.Mvc;
using FM.Domain.Entities;
using FM.Domain.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using FM.Application.DTOs.SubForumDTO;

namespace ApolloAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class SubForumController : ControllerBase
{
    private readonly ISubForumRepository _subForumRepository;
    private readonly ILogger<SubForumController> _logger;

    public SubForumController(ISubForumRepository subForumRepository, ILogger<SubForumController> logger)
    {
        _subForumRepository = subForumRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IEnumerable<SubForumDTO>> Get()
    {
        var subForums = await _subForumRepository.GetAllSubForumsAsync();
        return subForums.Select(sf => new SubForumDTO
        {
            Id = sf.Id,
            Name = sf.Name,
            Description = sf.Description
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SubForumDTO>> Get(int id)
    {
        var subForum = await _subForumRepository.GetSubForumByIdAsync(id);
        if (subForum == null)
        {
            return NotFound();
        }
        return new SubForumDTO
        {
            Id = subForum.Id,
            Name = subForum.Name,
            Description = subForum.Description
        };
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] SubForumCreateDTO subForumDto)
    {
        var subForum = new SubForum
        {
            Name = subForumDto.Name,
            Description = subForumDto.Description
        };

        await _subForumRepository.AddSubForumAsync(subForum);
        return CreatedAtAction(nameof(Get), new { id = subForum.Id }, new SubForumDTO
        {
            Id = subForum.Id,
            Name = subForum.Name,
            Description = subForum.Description
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, [FromBody] SubForumUpdateDTO subForumDto)
    {
        var subForum = await _subForumRepository.GetSubForumByIdAsync(id);
        if (subForum == null)
        {
            return NotFound();
        }

        subForum.Name = subForumDto.Name;
        subForum.Description = subForumDto.Description;

        await _subForumRepository.UpdateSubForumAsync(subForum);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _subForumRepository.DeleteSubForumAsync(id);
        return NoContent();
    }
}
