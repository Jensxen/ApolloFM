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

        async Task IPostCommand.CreatePostAsync(CreatePostCommandDTO command)
        {
            var userId = command.UserId?.Trim();

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("UserId cannot be null or empty");
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Since we know both ID fields are the same, we can just check one
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    // Simplified approach since IDs should match
                    throw new Exception($"User not found with ID: {userId}");
                }

                var subForum = await _subForumRepository.GetSubForumByIdAsync(command.SubForumId);
                if (subForum == null)
                {
                    throw new Exception("Sub Forum not found");
                }

                // Use the confirmed user ID
                var post = new Post(
                    command.Title,
                    command.Content,
                    string.IsNullOrEmpty(command.SpotifyPlaylistId) ? "none" : command.SpotifyPlaylistId,
                    userId, // Use the original ID directly since they're consistent
                    command.SubForumId
                );

                await _postRepository.AddPostAsync(post);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }



        async Task IPostCommand.UpdatePostAsync(UpdatePostCommandDTO command)
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

        async Task IPostCommand.DeletePostAsync(DeletePostCommandDTO command)
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
