using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FM.Application.Command.CommandDTO.CommentCommandDTO
{
    public class CreateCommentCommandDTO
    {
        public string Content { get; set; }
        public string UserId { get; set; }
        public int PostId { get; set; }
    }
}
