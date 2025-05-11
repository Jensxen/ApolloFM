using FM.Application.Services.ServiceDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FM.Application.Interfaces.IRepositories
{
    public interface IForumRepository
    {
        Task<List<ForumTopicDto>> GetAllTopicsAsync();
        Task<ForumTopicDto> GetTopicByIdAsync(int id);
        Task<List<SubForumDto>> GetAllSubForumsAsync();
        Task<ForumTopicDto> CreateTopicAsync(CreateTopicDto createTopicDto, string userId);
        Task<CommentDto> AddCommentAsync(AddCommentDto addCommentDto, string userId);
    }
}
