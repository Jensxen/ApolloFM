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
        public string Id { get; set; } // Primary key
        public string DisplayName { get; set; }
        public string SpotifyUserId { get; set; } // Spotify user ID

        public int UserRoleId { get; set; }
        public UserRole UserRole { get; set; }
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

}