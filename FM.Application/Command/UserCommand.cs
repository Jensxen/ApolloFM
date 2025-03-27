using FM.Application.Interfaces.ICommand;
using FM.Domain.Entities;
using FM.Application.Interfaces.IRepositories;
using FM.Application.Interfaces;
using FM.Application.Command.CommandDTO.UserCommandDTO;

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

        void IUserCommand.CreateUser(CreateUserCommandDTO command)
        {
            _unitOfWork.BeginTransaction();
            try
            {
                var user = new User
                {
                    DisplayName = command.DisplayName,
                    SpotifyUserId = command.SpotifyUserId,
                    UserRoleId = command.UserRoleId
                };

                _userRepository.AddUserAsync(user);
                _unitOfWork.Commit();
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        void IUserCommand.UpdateUser(UpdateUserCommandDTO command)
        {
            _unitOfWork.BeginTransaction();
            try
            {
                var user = _userRepository.GetUserByIdAsync(command.Id).Result;
                if (user == null)
                {
                    throw new Exception("User not found");
                }
                if (!user.RowVersion.SequenceEqual(command.RowVersion))
                {
                    throw new Exception("User has been modified by someone else. Please refresh and try again.");
                }
                user.DisplayName = command.DisplayName;
                user.SpotifyUserId = command.SpotifyUserId;
                user.UserRoleId = command.UserRoleId;

                _userRepository.UpdateUserAsync(user);
                _unitOfWork.Commit();
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        void IUserCommand.DeleteUser(DeleteUserCommandDTO command)
        {
            _unitOfWork.BeginTransaction();
            try
            {
                var user = _userRepository.GetUserByIdAsync(command.Id).Result;
                if (user == null)
                {
                    throw new Exception("User not found");
                }
                if (!user.RowVersion.SequenceEqual(command.RowVersion))
                {
                    throw new Exception("User has been modified by someone else. Please refresh and try again.");
                }

                _userRepository.DeleteUserAsync(command.Id);
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
