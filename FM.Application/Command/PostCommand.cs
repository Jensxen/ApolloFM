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

        public async Task CreatePostAsync(CreatePostCommandDTO command)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var user = await _userRepository.GetUserByIdAsync(command.UserId);
                var subForum = await _subForumRepository.GetSubForumByIdAsync(command.SubForumId);

                if (user == null)
                {
                    throw new Exception("User not found");
                }
                if (subForum == null)
                {
                    throw new Exception("Sub Forum not found");
                }

                var post = new Post(command.Title, command.Content, command.SpotifyPlaylistId, command.UserId, command.SubForumId);

                await _postRepository.AddPostAsync(post);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task UpdatePostAsync(UpdatePostCommandDTO command)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var post = await _postRepository.GetPostByIdAsync(command.Id);
                if (post == null)
                {
                    throw new Exception("Post not found");
                }
                if (!post.RowVersion.SequenceEqual(command.RowVersion))
                {
                    throw new Exception("The post has been modified by someone else. Please refresh and try again.");
                }

                post.UpdateTitle(command.Title);
                post.UpdateContent(command.Content);
                post.UpdateSpotifyPlaylistId(command.SpotifyPlaylistId);

                await _postRepository.UpdatePostAsync(post);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task DeletePostAsync(DeletePostCommandDTO command)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var post = await _postRepository.GetPostByIdAsync(command.Id);
                if (post == null)
                {
                    throw new Exception("Post not found");
                }
                if (!post.RowVersion.SequenceEqual(command.RowVersion))
                {
                    throw new Exception("The post has been modified by someone else. Please refresh and try again.");
                }

                await _postRepository.DeletePostAsync(command.Id);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }

}
