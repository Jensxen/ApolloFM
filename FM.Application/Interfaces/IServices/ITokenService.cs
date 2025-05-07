using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FM.Application.Interfaces.IServices
{
    public interface ITokenService
    {
        Task<string> GetAccessToken();
        Task SetAccessToken(string accessToken, int expiresInSeconds);
        Task<string> GetRefreshToken();
        Task SetRefreshToken(string refreshToken);
        Task<bool> NeedsRefresh();
        Task ClearAccessTokens();
    }

}
