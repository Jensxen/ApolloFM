using FM.Domain.Entities;

namespace FM.Application.Interfaces.IRepositories
{
    public interface IUserRoleRepository
    {
        Task<UserRole> GetUserRoleByIdAsync(int id);
    }
}
