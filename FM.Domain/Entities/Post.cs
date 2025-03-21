using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FM.Domain.Entities
{
    public class Post
    {
        public int Id { get; set; } // Primary key
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string SpotifyPlaylistId { get; set; } // Spotify playlist ID

        // Foreign key and navigation property for User
        public string UserId { get; set; }
        public User User { get; set; }

        public int SubForumId { get; set; }
        public SubForum SubForum { get; set; }
    }
}