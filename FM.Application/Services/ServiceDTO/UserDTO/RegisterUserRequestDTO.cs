using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FM.Application.Services.ServiceDTO.UserDTO
{
    // ApolloAPI/Models/RegisterUserRequest.cs
    public class RegisterUserRequestDTO
    {
        public string SpotifyUserId { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string ProfileImageUrl { get; set; }
    }

}
