using FM.Domain.Entities;
using FM.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using FM.Application.Interfaces.IRepositories;

namespace FM.Infrastucture.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly ApolloContext _context;
        public PostRepository(ApolloContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Post>> GetAllPostsAsync()
        {
            return await _context.Set<Post>()
                .Include(p => p.Comments) // Include comments
                .ThenInclude(c => c.User) // Include related User entity for comments
                .Include(p => p.User) // Include related User entity for posts
                .Include(p => p.SubForum) // Include related SubForum entity
                .ToListAsync();
        }

        public async Task<Post> GetPostByIdAsync(int id)
        {
            return await _context.Set<Post>()
                .Include(p => p.Comments) // Include comments
                .ThenInclude(c => c.User) // Include related User entity for comments
                .Include(p => p.User) // Include related User entity for posts
                .Include(p => p.SubForum) // Include related SubForum entity
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddPostAsync(Post post)
        {
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePostAsync(Post post)
        {
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePostAsync(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }
        }
    }
}