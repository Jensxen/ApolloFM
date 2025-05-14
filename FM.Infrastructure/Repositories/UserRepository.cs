using FM.Application.Interfaces.IRepositories;
using FM.Domain.Entities;
using FM.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FM.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApolloContext _context;

        public UserRepository(ApolloContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetBySpotifyIdAsync(string spotifyId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.SpotifyUserId == spotifyId);
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task<IEnumerable<User>> GetAllAsync(int limit = 100)
        {
            return await _context.Users.Take(limit).ToListAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
        }

        public async Task DeleteUserAsync(string id, byte[] rowVersion)
        {
            // Option 1: If User has soft delete functionality
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return;

            // Check if rowVersion matches
            if (!user.RowVersion.SequenceEqual(rowVersion))
                throw new DbUpdateConcurrencyException("The user has been modified by someone else.");

            _context.Users.Remove(user);
        }

        public async Task<User?> GetFirstUserAsync()
        {
            return await _context.Users.FirstOrDefaultAsync();
        }
    }
}
