using FM.Domain.Entities;
using FM.Application.Interfaces.IRepositories;
using FM.Application.Interfaces.ICommand;
using FM.Application.Interfaces;
using FM.Application.Command.CommandDTO.CommentCommandDTO;

namespace FM.Application.Command
{
    public class CommentCommand : ICommentCommand
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;

        public CommentCommand(ICommentRepository commentRepository, IUnitOfWork unitOfWork, IUserRepository userRepository, IPostRepository postRepository)
        {
            _commentRepository = commentRepository;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _postRepository = postRepository;
        }

        async Task ICommentCommand.CreateCommentAsync(CreateCommentCommandDTO command)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var user = await _userRepository.GetByIdAsync(command.UserId);
                var post = await _postRepository.GetPostByIdAsync(command.PostId);

                if (user == null)
                {
                    throw new Exception("User not found");
                }
                if (post == null)
                {
                    throw new Exception("Post not found");
                }

                var comment = new Comment(command.Content, command.UserId, command.PostId);

                await _commentRepository.AddCommentAsync(comment);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        async Task ICommentCommand.UpdateCommentAsync(UpdateCommentCommandDTO command)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var comment = await _commentRepository.GetCommentByIdAsync(command.Id);
                if (comment == null)
                {
                    throw new Exception("Comment not found");
                }
                if (!comment.RowVersion.SequenceEqual(command.RowVersion))
                {
                    throw new Exception("The comment has been modified by someone else. Please refresh and try again.");
                }

                comment.UpdateContent(command.Content);

                await _commentRepository.UpdateCommentAsync(comment);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        async Task ICommentCommand.DeleteCommentAsync(DeleteCommentCommandDTO command)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var comment = await _commentRepository.GetCommentByIdAsync(command.Id);
                if (comment == null)
                {
                    throw new Exception("Comment not found");
                }
                if (!comment.RowVersion.SequenceEqual(command.RowVersion))
                {
                    throw new Exception("The comment has been modified by someone else. Please refresh and try again.");
                }

                await _commentRepository.DeleteCommentAsync(command.Id);
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
