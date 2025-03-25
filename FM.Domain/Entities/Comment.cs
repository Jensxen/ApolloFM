using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FM.Domain.Entities
{
    public class Comment
    {
        public int Id { get; set; } // Primary key
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        // Foreign key and navigation property for Post
        public int PostId { get; set; }
        public Post Post { get; set; }

        // Foreign key and navigation property for User
        public string UserId { get; set; }
        public User User { get; set; }
    }

}