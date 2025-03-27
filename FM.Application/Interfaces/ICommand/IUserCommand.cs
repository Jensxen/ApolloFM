using FM.Application.Command.CommandDTO;
using FM.Application.Command.CommandDTO.UserCommandDTO;

namespace FM.Application.Interfaces.ICommand
{
    public interface IUserCommand
    {
        void CreateUser(CreateUserCommandDTO command);
        void UpdateUser(UpdateUserCommandDTO command);
        void DeleteUser(DeleteUserCommandDTO command);
    }
}
