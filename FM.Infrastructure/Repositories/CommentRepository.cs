using FM.Application.Interfaces.IRepositories;
using FM.Domain.Entities;
using FM.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FM.Infrastructure.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ApolloContext _dbContext;

        public CommentRepository(ApolloContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Comment> GetCommentByIdAsync(int id)
        {
            return await _dbContext.Set<Comment>()
                .Include(c => c.User)
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddCommentAsync(Comment comment)
        {
            await _dbContext.Set<Comment>().AddAsync(comment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateCommentAsync(Comment comment)
        {
            _dbContext.Set<Comment>().Update(comment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteCommentAsync(int id)
        {
            var comment = await GetCommentByIdAsync(id);
            if (comment != null)
            {
                _dbContext.Set<Comment>().Remove(comment);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Comment>> GetAllCommentsAsync()
        {
            return await _dbContext.Set<Comment>()
                .Include(c => c.User) 
                .Include(c => c.Post) 
                .ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(int postId)
        {
            return await _dbContext.Set<Comment>()
                .Where(c => c.PostId == postId) 
                .Include(c => c.User) 
                .ToListAsync();
        }
    }
}
