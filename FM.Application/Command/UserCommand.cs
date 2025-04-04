using FM.Application.Interfaces.ICommand;
using FM.Domain.Entities;
using FM.Application.Interfaces.IRepositories;
using FM.Application.Interfaces;
using FM.Application.Command.CommandDTO.UserCommandDTO;
using System.Threading.Tasks;

namespace FM.Application.Command
{
    public class UserCommand : IUserCommand
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UserCommand(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task CreateUserAsync(CreateUserCommandDTO command)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var user = new User(Guid.NewGuid().ToString(), command.DisplayName, command.SpotifyUserId, command.UserRoleId);

                await _userRepository.AddUserAsync(user);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateUserAsync(UpdateUserCommandDTO command)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var user = await _userRepository.GetUserByIdAsync(command.Id);
                if (user == null)
                {
                    throw new Exception("User not found");
                }

                if (!user.RowVersion.SequenceEqual(command.RowVersion))
                {
                    throw new Exception("The user has been modified by someone else. Please refresh and try again.");
                }

                user.UpdateDisplayName(command.DisplayName);
                user.UpdateSpotifyUserId(command.SpotifyUserId);
                user.UpdateUserRoleId(command.UserRoleId);

                await _userRepository.UpdateUserAsync(user);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteUserAsync(DeleteUserCommandDTO command)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var user = await _userRepository.GetUserByIdAsync(command.Id);
                if (user == null)
                {
                    throw new Exception("User not found");
                }

                if (!user.RowVersion.SequenceEqual(command.RowVersion))
                {
                    throw new Exception("The user has been modified by someone else. Please refresh and try again.");
                }

                await _userRepository.DeleteUserAsync(command.Id, user.RowVersion);
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
