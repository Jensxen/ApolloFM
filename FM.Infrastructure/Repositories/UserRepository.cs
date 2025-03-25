using FM.Domain.Entities;
using FM.Application.Interfaces;
using FM.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace FM.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApolloContext _context;

        public UserRepository(ApolloContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.UserRole)
                .ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(string id)
        {
            return await _context.Users
                .Include(u => u.UserRole)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}