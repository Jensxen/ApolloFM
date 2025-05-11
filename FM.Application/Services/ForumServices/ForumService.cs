// FM.Application/Services/ForumServices/ForumService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FM.Application.Interfaces.IRepositories;
using FM.Application.Services.ServiceDTO;

namespace FM.Application.Services.ForumServices
{
    public class ForumService : IForumService
    {
        private readonly IForumRepository _forumRepository;

        public ForumService(IForumRepository forumRepository)
        {
            _forumRepository = forumRepository;
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

        public async Task<ForumTopicDto> CreateTopicAsync(CreateTopicDto createTopicDto, string userId)
        {
            if (string.IsNullOrEmpty(createTopicDto.Title))
                throw new ArgumentException("Topic title cannot be empty");

            if (string.IsNullOrEmpty(createTopicDto.Content))
                throw new ArgumentException("Topic content cannot be empty");

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
