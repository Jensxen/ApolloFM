using FM.Application.Interfaces.IQuery;
using FM.Application.QueryDTO.SubForumDTO;
using FM.Application.Interfaces.IRepositories;

namespace FM.Application.Query
{
    public class SubForumQuery : ISubForumQuery
    {
        private readonly ISubForumRepository _subForumRepository;

        public SubForumQuery(ISubForumRepository subForumRepository)
        {
            _subForumRepository = subForumRepository;
        }

        public async Task<IEnumerable<SubForumQueryDTO>> GetAllSubForumsAsync()
        {
            var subForums = await _subForumRepository.GetAllSubForumsAsync();
            return subForums.Select(sf => new SubForumQueryDTO
            {
                Id = sf.Id,
                Name = sf.Name,
                Description = sf.Description
            });
        }

        public async Task<SubForumQueryDTO> GetSubForumByIdAsync(int id)
        {
            var subForum = await _subForumRepository.GetSubForumByIdAsync(id);
            if (subForum == null)
            {
                return null;
            }
            return new SubForumQueryDTO
            {
                Id = subForum.Id,
                Name = subForum.Name,
                Description = subForum.Description
            };
        }
    }
}



