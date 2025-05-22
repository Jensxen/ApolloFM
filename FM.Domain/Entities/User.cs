using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FM.Domain.Entities
{
    public class User
    {
        public string Id { get; protected set; }
        public string DisplayName { get; protected set; }
        public string SpotifyUserId { get; protected set; } 
        public int UserRoleId { get; protected set; }
        public UserRole? Role { get; protected set; }

        [Timestamp]
        public byte[] RowVersion { get; protected set; }


        protected User() { }
        public User(string id, string displayName, string spotifyUserId, int userRoleId)
        {
            Id = id;
            DisplayName = displayName;
            SpotifyUserId = spotifyUserId;
            UserRoleId = userRoleId;
        }

        public void UpdateDisplayName(string displayName)
        {
            if (!string.IsNullOrEmpty(displayName))
            {
                DisplayName = displayName;
            }
        }

        public void UpdateSpotifyUserId(string spotifyUserId)
        {
            SpotifyUserId = spotifyUserId;
        }

        public void UpdateUserRoleId(int userRoleId)
        {
            UserRoleId = userRoleId;
        }
    }

}