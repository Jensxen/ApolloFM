using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FM.Application.Services.UserServices
{
    public interface IUserService
    {
        Task<string> GetCurrentUserIdAsync();
        string GetCurrentUserId();
    }
}
