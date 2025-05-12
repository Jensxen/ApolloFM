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
            // Normalize the userId and log key information
            var originalUserId = command.UserId?.Trim();
            Console.WriteLine($"Creating post with UserId: '{originalUserId}'");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Check if we have a direct ID match
                var user = await _userRepository.GetByIdAsync(originalUserId);
                Console.WriteLine($"GetByIdAsync result for '{originalUserId}': {(user != null ? "Found" : "Not Found")}");

                // If not, try finding by SpotifyId
                if (user == null)
                {
                    Console.WriteLine($"Looking for user by SpotifyId: '{originalUserId}'");
                    user = await _userRepository.GetBySpotifyIdAsync(originalUserId);
                    Console.WriteLine($"GetBySpotifyIdAsync result: {(user != null ? "Found" : "Not Found")}");

                    if (user != null)
                    {
                        // HERE IS THE KEY FIX! Use the actual database ID from the User table
                        command.UserId = user.Id;
                        Console.WriteLine($"IMPORTANT: Using database ID from found user: '{user.Id}' instead of '{originalUserId}'");
                    }
                    else
                    {
                        // No user found at all - check what IDs exactly we're using
                        Console.WriteLine("ERROR: User not found by any method. This is a critical error.");
                        Console.WriteLine($"Original UserId: '{originalUserId}'");

                        // Specific diagnostic for popular Spotify IDs
                        if (originalUserId?.Length == 25 && !originalUserId.Contains("-"))
                        {
                            Console.WriteLine("This appears to be a Spotify ID format (25 chars, no hyphens)");
                            Console.WriteLine("The database is likely expecting a different ID format (e.g., UUID/GUID with hyphens)");
                        }

                        throw new Exception($"User not found with ID: {originalUserId}");
                    }
                }

                var subForum = await _subForumRepository.GetSubForumByIdAsync(command.SubForumId);
                if (subForum == null)
                {
                    throw new Exception("Sub Forum not found");
                }

                // CRITICAL: Use the correct UserId - potentially translated from SpotifyId to DB Id
                Console.WriteLine($"Creating Post with UserId: '{command.UserId}'");
                var post = new Post(
                    command.Title,
                    command.Content,
                    string.IsNullOrEmpty(command.SpotifyPlaylistId) ? "none" : command.SpotifyPlaylistId,
                    command.UserId,  // This should now be the correct DB ID
                    command.SubForumId
                );

                await _postRepository.AddPostAsync(post);
                Console.WriteLine("About to commit transaction...");
                await _unitOfWork.CommitAsync();
                Console.WriteLine("Transaction committed successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                Console.WriteLine($"ERROR in CreatePostAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

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
