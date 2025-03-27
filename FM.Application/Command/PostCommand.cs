using FM.Domain.Entities;
using FM.Application.Interfaces.IRepositories;
using FM.Application.Interfaces.ICommand;
using FM.Application.Interfaces;
using FM.Application.Command.CommandDTO.PostCommandDTO;

namespace FM.Application.Command
{
    public class PostCommand : IPostCommand
    {
        private readonly IPostRepository _postRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        public readonly ISubForumRepository _subForumRepository;

        public PostCommand(IPostRepository postRepository, IUnitOfWork unitOfWork, IUserRepository userRepository, ISubForumRepository subForumRepository)
        {
            _postRepository = postRepository;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _subForumRepository = subForumRepository;
        }

        void IPostCommand.CreatePost(CreatePostCommandDTO command)
        {
            _unitOfWork.BeginTransaction();
            try
            {
                var user = _userRepository.GetUserByIdAsync(command.UserId).Result;
                var subForum = _subForumRepository.GetSubForumByIdAsync(command.SubForumId).Result;

                if (user == null)
                {
                    throw new Exception("User not found");
                }
                if (subForum == null)
                {
                    throw new Exception("Sub Forum not found");
                }

                var post = new Post()
                {
                    Title = command.Title,
                    Content = command.Content,
                    CreatedAt = DateTime.UtcNow,
                    SpotifyPlaylistId = command.SpotifyPlaylistId,
                    UserId = command.UserId,
                    SubForumId = command.SubForumId
                };

                _postRepository.AddPostAsync(post);
                _unitOfWork.Commit();
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        void IPostCommand.UpdatePost(UpdatePostCommandDTO command)
        {
            _unitOfWork.BeginTransaction();
            try
            {
                var post = _postRepository.GetPostByIdAsync(command.Id).Result;
                if (post == null)
                {
                    throw new Exception("Post not found");
                }
                if (post.RowVersion != command.RowVersion)
                {
                    throw new Exception("The post has been modified by someone else. Please refresh and try again.");
                }

                post.Title = command.Title;
                post.Content = command.Content;
                post.SpotifyPlaylistId = command.SpotifyPlaylistId;

                _postRepository.UpdatePostAsync(post);
                _unitOfWork.Commit();

            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        void IPostCommand.DeletePost(DeletePostCommandDTO command)
        {
            _unitOfWork.BeginTransaction();
            try
            {
                var post = _postRepository.GetPostByIdAsync(command.Id).Result;
                if (post == null)
                {
                    throw new Exception("Post not found");
                }
                if (post.RowVersion != command.RowVersion)
                {
                    throw new Exception("The post has been modified by someone else. Please refresh and try again.");
                }

                _postRepository.DeletePostAsync(command.Id);
                _unitOfWork.Commit();
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }
    }

}
