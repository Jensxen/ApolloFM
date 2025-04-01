using System.Collections.Generic;
using System.Threading.Tasks;
using FM.Application.Interfaces.IQuery;
using FM.Application.Interfaces.IRepositories;
using FM.Application.QueryDTO.PostDTO;

namespace FM.Application.Query
{
    public class PostQuery : IPostQuery
    {
        private readonly IPostRepository _postRepository;

        public PostQuery(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        public async Task<IEnumerable<PostQueryDTO>> GetAllPostsAsync()
        {
            var posts = await _postRepository.GetAllPostsAsync();
            return posts.Select(p => new PostQueryDTO
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                CreatedAt = p.CreatedAt,
                SpotifyPlaylistId = p.SpotifyPlaylistId,
                UserId = p.UserId,
                SubForumId = p.SubForumId
            });
        }

        public async Task<PostQueryDTO> GetPostByIdAsync(int id)
        {
            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null)
            {
                return null;
            }
            return new PostQueryDTO
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                SpotifyPlaylistId = post.SpotifyPlaylistId,
                UserId = post.UserId,
                SubForumId = post.SubForumId
            };
        }
    }
}
