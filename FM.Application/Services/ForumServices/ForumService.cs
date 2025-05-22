// FM.Application/Services/ForumServices/ForumService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FM.Application.Interfaces.IRepositories;
using FM.Application.Services.ServiceDTO;
using FM.Application.Services.UserServices;

namespace FM.Application.Services.ForumServices
{
    public class ForumService : IForumService
    {
        private readonly IForumRepository _forumRepository;
        private readonly IUserService _userContext;
        private readonly IUserRepository _userRepository;

        public ForumService(IForumRepository forumRepository, IUserService userContext, IUserRepository userRepository)
        {
            _forumRepository = forumRepository;
            _userContext = userContext;
            _userRepository = userRepository;
        }

        public async Task<List<ForumTopicDto>> GetTopicsAsync()
        {
            return await _forumRepository.GetAllTopicsAsync();
        }

        public async Task<ForumTopicDto> GetTopicByIdAsync(int id)
        {
            return await _forumRepository.GetTopicByIdAsync(id);
        }

        public async Task<List<SubForumDto>> GetSubForumsAsync()
        {
            return await _forumRepository.GetAllSubForumsAsync();
        }

        public async Task<ForumTopicDto> CreateTopicAsync(CreateTopicDto createTopicDto)
        {
            if (string.IsNullOrEmpty(createTopicDto.Title))
                throw new ArgumentException("Topic title cannot be empty");

            if (string.IsNullOrEmpty(createTopicDto.Content))
                throw new ArgumentException("Topic content cannot be empty");

            var userId = await _userContext.GetCurrentUserIdAsync();

            return await _forumRepository.CreateTopicAsync(createTopicDto, userId);
        }

        public async Task<ForumTopicDto> CreateTopicWithUserIdAsync(CreateTopicDto createTopicDto, string userId)
        {
            if (string.IsNullOrEmpty(createTopicDto.Title))
                throw new ArgumentException("Topic title cannot be empty");

            if (string.IsNullOrEmpty(createTopicDto.Content))
                throw new ArgumentException("Topic content cannot be empty");

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User must be authenticated to create a topic");

            return await _forumRepository.CreateTopicAsync(createTopicDto, userId);
        }

        public async Task<CommentDto> AddCommentAsync(AddCommentDto addCommentDto, string userId)
        {
            if (string.IsNullOrEmpty(addCommentDto.Content))
                throw new ArgumentException("Comment content cannot be empty");

            return await _forumRepository.AddCommentAsync(addCommentDto, userId);
        }
    }
}
