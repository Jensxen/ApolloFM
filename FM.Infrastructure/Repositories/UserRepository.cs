using System.Threading.Tasks;
using FM.Application.Interfaces.IRepositories;
using FM.Domain.Entities;
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

        public async Task<User> GetUserByIdAsync(string id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(string id, byte[] rowVersion)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            _context.Entry(user).Property(u => u.RowVersion).OriginalValue = rowVersion;
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}


