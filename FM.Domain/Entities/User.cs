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
        public string Id { get; protected set; } // Primary key
        public string DisplayName { get; protected set; }
        public string SpotifyUserId { get; protected set; } // Spotify user ID
        public int UserRoleId { get; protected set; }
        public UserRole UserRole { get; protected set; }
        public ICollection<Post> Posts { get; protected set; } = new List<Post>();
        public ICollection<Comment> Comments { get; protected set; } = new List<Comment>();

        [Timestamp]
        public byte[] RowVersion { get; protected set; }


        protected User() { }

        // Constructor to set initial values including Id
        public User(string id, string displayName, string spotifyUserId, int userRoleId)
        {
            Id = id;
            DisplayName = displayName;
            SpotifyUserId = spotifyUserId;
            UserRoleId = userRoleId;
        }

        public void UpdateDisplayName(string displayName)
        {
            DisplayName = displayName;
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