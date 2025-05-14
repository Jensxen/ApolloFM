using FM.Domain.Entities;
using System.Threading.Tasks;

namespace FM.Application.Interfaces.IRepositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetBySpotifyIdAsync(string spotifyId);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(string id, byte[] rowVersion);
        Task<IEnumerable<User>> GetAllAsync(int limit = 100);
        Task<User> GetFirstUserAsync();
    }
}
