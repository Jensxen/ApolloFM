using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FM.Application.Command.CommandDTO.PostCommandDTO
{
   public class CreatePostCommandDTO
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string SpotifyPlaylistId { get; set; }
        public string UserId { get; set; }
        public int SubForumId { get; set; }
    }
}
