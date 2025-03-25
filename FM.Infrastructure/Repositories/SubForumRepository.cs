using FM.Domain.Entities;
using FM.Application.Interfaces;
using FM.Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FM.Infrastructure.Repositories
{
    public class SubForumRepository : ISubForumRepository
    {
        private readonly ApolloContext _context;

        public SubForumRepository(ApolloContext context)
        {
            _context = context;
        }

        public async Task AddSubForumAsync(SubForum subForum)
        {
            _context.SubForums.Add(subForum);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSubForumAsync(int id)
        {
            var post = await _context.SubForums.FindAsync(id);
            if (post != null)
            {
                _context.SubForums.Remove(post);
            }
        }

        public async Task<IEnumerable<SubForum>> GetAllSubForumsAsync()
        {
            return await _context.SubForums
                .Include(s => s.Posts)
                .ToListAsync();
        }

        public async Task<SubForum> GetSubForumByIdAsync(int id)
        {
            return await _context.SubForums
                .Include(s => s.Posts)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task UpdateSubForumAsync(SubForum subForum)
        {
            _context.SubForums.Update(subForum);
            await _context.SaveChangesAsync();
        }
    }
}
