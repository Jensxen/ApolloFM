using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FM.Domain.Entities
{
    public class Post
    {
        public int Id { get; protected set; } // Primary key
        public string Title { get; protected set; }
        public string Content { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public string SpotifyPlaylistId { get; protected set; } // Spotify playlist ID

        // Foreign key and navigation property for User
        public string UserId { get; protected set; }
        public User User { get; protected set; }

        // Foreign key and navigation property for SubForum
        public int SubForumId { get; protected set; }
        public SubForum SubForum { get; protected set; }

        public ICollection<Comment> Comments { get; protected set; } = new List<Comment>();

        [Timestamp]
        public byte[] RowVersion { get; protected set; }

        public Post(string title, string content, string spotifyPlaylistId, string userId, int subForumId)
        {
            Title = title;
            Content = content;
            CreatedAt = DateTime.UtcNow;
            SpotifyPlaylistId = spotifyPlaylistId;
            UserId = userId;
            SubForumId = subForumId;
        }

        public void UpdateTitle(string title)
        {
            Title = title;
        }

        public void UpdateContent(string content)
        {
            Content = content;
        }

        public void UpdateSpotifyPlaylistId(string spotifyPlaylistId)
        {
            SpotifyPlaylistId = spotifyPlaylistId;
        }

    }

}