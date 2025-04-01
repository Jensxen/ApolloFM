using FM.Application.QueryDTO.PostDTO;

namespace FM.Application.Interfaces.IQuery
{
    public interface IPostQuery
    {
        Task<IEnumerable<PostQueryDTO>> GetAllPostsAsync();
        Task<PostQueryDTO> GetPostByIdAsync(int id);
    }
}
