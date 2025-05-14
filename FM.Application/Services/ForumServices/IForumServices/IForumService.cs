using FM.Application.Services.ServiceDTO;

namespace FM.Application.Services.ForumServices
{
    public interface IForumService
    {
        Task<List<ForumTopicDto>> GetTopicsAsync();
        Task<ForumTopicDto> GetTopicByIdAsync(int id);
        Task<List<SubForumDto>> GetSubForumsAsync();
        Task<ForumTopicDto> CreateTopicAsync(CreateTopicDto createTopicDto);
        Task<CommentDto> AddCommentAsync(AddCommentDto addCommentDto, string userId);
        Task<ForumTopicDto> CreateTopicWithUserIdAsync(CreateTopicDto createTopicDto, string userId);
    }

}
