using FM.Application.Interfaces.IRepositories;
using FM.Domain.Entities;
using System.Net.Http.Json;

namespace FM.Infrastructure.Repositories.WebAssembly
{
    public class WebUserRepository : IUserRepository
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public WebUserRepository(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<User?> GetFirstUserAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApolloAPI");
                return await client.GetFromJsonAsync<User>("api/users/first");
            }
            catch
            {
                // Return a default user for client-side operations
                return new User("default_user", "Default User", "default_spotify_id", 1);
            }
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApolloAPI");
                return await client.GetFromJsonAsync<User>($"api/users/{id}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<User?> GetBySpotifyIdAsync(string spotifyId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApolloAPI");
                return await client.GetFromJsonAsync<User>($"api/users/spotify/{spotifyId}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<User>> GetAllAsync(int limit = 100)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApolloAPI");
                var users = await client.GetFromJsonAsync<List<User>>($"api/users?limit={limit}");
                return users ?? new List<User>();
            }
            catch
            {
                return new List<User>();
            }
        }

        // These methods don't need real implementations in the WebAssembly client
        public Task AddUserAsync(User user) => Task.CompletedTask;
        public Task UpdateUserAsync(User user) => Task.CompletedTask;
        public Task DeleteUserAsync(string id, byte[] rowVersion) => Task.CompletedTask;
    }
}