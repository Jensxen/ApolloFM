using FM.Application.Interfaces.IQuery;
using FM.Application.Interfaces.IRepositories;
using FM.Application.QueryDTO.CommentDTO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FM.Application.Query
{
    public class CommentQuery : ICommentQuery
    {
        private readonly ICommentRepository _commentRepository;

        public CommentQuery(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<IEnumerable<CommentQueryDTO>> GetAllCommentsAsync()
        {
            var comments = await _commentRepository.GetAllCommentsAsync();
            return comments.Select(c => new CommentQueryDTO
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                UserId = c.UserId,
                PostId = c.PostId
            });
        }

        public async Task<IEnumerable<CommentQueryDTO>> GetCommentsByPostIdAsync(int postId)
        {
            var comments = await _commentRepository.GetCommentsByPostIdAsync(postId);
            return comments.Select(c => new CommentQueryDTO
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                UserId = c.UserId,
                PostId = c.PostId
            });
        }

        public async Task<CommentQueryDTO> GetCommentByIdAsync(int id)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(id);
            if (comment == null)
            {
                return null;
            }
            return new CommentQueryDTO
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UserId = comment.UserId,
                PostId = comment.PostId
            };
        }
    }
}
