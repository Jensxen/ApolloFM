// FM.Application/Interfaces/IServices/IAuthService.cs
using FM.Application.Services.ServiceDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FM.Application.Interfaces.IServices
{
    public interface IAuthService
    {
        Task<bool> IsUserAuthenticated();
        Task<SpotifyUserProfile> GetUserProfile();
        Task<List<SpotifyDataDTO>> GetTopTracksAsync(string timeRange = "medium_term");
        Task<SpotifyDataDTO> GetCurrentlyPlayingTrackAsync();
        Task<string> GetValidAccessTokenAsync();
        
        // These methods might differ between implementations
        Task LogoutAsync();
        Task LoginAsync();
    }
}
