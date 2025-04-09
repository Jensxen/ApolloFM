using System;
using System.Collections.Generic;
using System.Linq;
namespace FM.Domain.Entities
{
    public class SpotifyUserProfile
    {
        public string DisplayName { get; protected set; }
        public string Email { get; protected set; }
        public string Id { get; protected set; }

        
        public SpotifyUserProfile() { }

        public SpotifyUserProfile(string id, string displayName, string email)
        {
            Id = id;
            DisplayName = displayName;
            Email = email;
        }
    }
}
