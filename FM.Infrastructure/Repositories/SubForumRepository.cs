using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FM.Application.Interfaces.IRepositories;
using FM.Domain.Entities;
using FM.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace FM.Infrastructure.Repositories
{
    public class SubForumRepository : ISubForumRepository
    {
        private readonly ApolloContext _context;

        public SubForumRepository(ApolloContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SubForum>> GetAllSubForumsAsync()
        {
            return await _context.SubForums.ToListAsync();
        }

        public async Task<SubForum> GetSubForumByIdAsync(int id)
        {
            return await _context.SubForums.FindAsync(id);
        }

        public async Task AddSubForumAsync(SubForum subForum)
        {
            await _context.SubForums.AddAsync(subForum);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSubForumAsync(SubForum subForum)
        {
            _context.SubForums.Update(subForum);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSubForumAsync(int id)
        {
            var subForum = await _context.SubForums.FindAsync(id);
            if (subForum != null)
            {
                _context.SubForums.Remove(subForum);
                await _context.SaveChangesAsync();
            }
        }
    }
}

