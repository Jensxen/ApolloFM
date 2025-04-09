using FM.Application.Command.CommandDTO.CommentCommandDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FM.Application.Interfaces.ICommand
{
    public interface ICommentCommand
    {
        public Task CreateCommentAsync(CreateCommentCommandDTO command);
        public Task UpdateCommentAsync(UpdateCommentCommandDTO command);
        public Task DeleteCommentAsync(DeleteCommentCommandDTO command);
    }

}
