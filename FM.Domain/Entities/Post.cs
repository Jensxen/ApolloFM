using FM.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FM.Domain.Entities
{
    public class Post
    {
        public int Id { get; protected set; }
        public string Title { get; protected set; }
        public string Content { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }
        public string SpotifyPlaylistId { get; protected set; }
        public string Icon { get; protected set; }

        // Add these missing properties
        public int PostTypeId { get; protected set; }
        public int? ParentPostId { get; protected set; }
        public Post ParentPost { get; protected set; }

        // Foreign key for User
        public string UserId { get; protected set; }
        public User User { get; protected set; }

        // Foreign key for SubForum
        public int SubForumId { get; protected set; }
        public SubForum SubForum { get; protected set; }

        public ICollection<Comment> Comments { get; protected set; } = new List<Comment>();

        [Timestamp]
        public byte[] RowVersion { get; protected set; }

        // Default constructor for EF Core
        public Post()
        {
            CreatedAt = DateTime.UtcNow;
        }

        // Constructor for topic posts
        public Post(string title, string content, string spotifyPlaylistId, string userId, int subForumId)
        {
            Title = title;
            Content = content;
            SpotifyPlaylistId = spotifyPlaylistId;
            UserId = userId;
            SubForumId = subForumId;
            PostTypeId = (int)PostTypeEnum.Topic;
            CreatedAt = DateTime.UtcNow;
        }



        // Constructor for comment posts
        public Post(string content, string userId, int parentPostId)
        {
            Content = content;
            UserId = userId;
            ParentPostId = parentPostId;
            PostTypeId = (int)PostTypeEnum.Comment;
            CreatedAt = DateTime.UtcNow;
        }

        // Update methods
        public void UpdateTitle(string title)
        {
            Title = title;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateContent(string content)
        {
            Content = content;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateSpotifyPlaylistId(string spotifyPlaylistId)
        {
            SpotifyPlaylistId = spotifyPlaylistId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateIcon(string icon)
        {
            Icon = icon;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}