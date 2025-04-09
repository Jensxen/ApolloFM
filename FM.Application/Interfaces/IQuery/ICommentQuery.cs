using FM.Application.QueryDTO.CommentDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FM.Application.Interfaces.IQuery
{
    public interface ICommentQuery
    {
        Task<IEnumerable<CommentQueryDTO>> GetAllCommentsAsync();
        Task<IEnumerable<CommentQueryDTO>> GetCommentsByPostIdAsync(int postId);
        Task<CommentQueryDTO> GetCommentByIdAsync(int id);
    }
}