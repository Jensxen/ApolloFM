using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FM.Domain.Entities;

namespace FM.Domain.Interfaces
{
    public interface ISubForumRepository
    {
        Task<IEnumerable<SubForum>> GetAllSubForums();
        Task<SubForum> GetSubForumById(int id);
        Task AddSubForum(SubForum subForum);
        Task UpdateSubForum(SubForum subForum);
        Task DeleteSubForum(int id);
    }
}
