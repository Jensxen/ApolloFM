using FM.Application.Command.CommandDTO;
using FM.Application.Command.CommandDTO.UserCommandDTO;

namespace FM.Application.Interfaces.ICommand
{
    public interface IUserCommand
    {
        Task CreateUserAsync(CreateUserCommandDTO command);
        Task UpdateUserAsync(UpdateUserCommandDTO command);
        Task DeleteUserAsync(DeleteUserCommandDTO command);
    }
}
