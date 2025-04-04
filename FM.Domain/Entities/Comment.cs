using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FM.Domain.Entities
{
    public class Comment
    {
        public int Id { get; protected set; } // Primary key
        public string Content { get; protected set; }
        public DateTime CreatedAt { get; protected set; }

        // Foreign key and navigation property for Post
        public int PostId { get; protected set; }
        public Post Post { get; protected set; }

        // Foreign key and navigation property for User
        public string UserId { get; protected set; }
        public User User { get; protected set; }

        public byte[] RowVersion { get; protected set; }

        public void UpdateContent(string content)
        {
            Content = content;
        }

    }

}