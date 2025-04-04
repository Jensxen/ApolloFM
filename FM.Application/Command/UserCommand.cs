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
                var user = new User
                {
                    Id = Guid.NewGuid().ToString(), // Set the Id to a new GUID
                    DisplayName = command.DisplayName,
                    SpotifyUserId = command.SpotifyUserId,
                    UserRoleId = command.UserRoleId
                };

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

                user.DisplayName = command.DisplayName;
                user.SpotifyUserId = command.SpotifyUserId;
                user.UserRoleId = command.UserRoleId;

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
