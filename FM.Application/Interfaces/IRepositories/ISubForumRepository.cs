using System.Collections.Generic;
using System.Threading.Tasks;
using FM.Domain.Entities;

namespace FM.Application.Interfaces.IRepositories
{
    public interface ISubForumRepository
    {
        Task<IEnumerable<SubForum>> GetAllSubForumsAsync();
        Task<SubForum> GetSubForumByIdAsync(int id);
        Task AddSubForumAsync(SubForum subForum);
        Task UpdateSubForumAsync(SubForum subForum);
        Task DeleteSubForumAsync(int id);
    }
}

