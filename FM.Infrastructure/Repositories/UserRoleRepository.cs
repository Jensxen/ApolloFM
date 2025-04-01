using System.Threading.Tasks;
using FM.Application.Interfaces.IRepositories;
using FM.Domain.Entities;
using FM.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace FM.Infrastructure.Repositories
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly ApolloContext _context;

        public UserRoleRepository(ApolloContext context)
        {
            _context = context;
        }

        public async Task<UserRole> GetUserRoleByIdAsync(int id)
        {
            return await _context.UserRoles.FindAsync(id);
        }
    }
}


