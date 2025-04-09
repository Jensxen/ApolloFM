using FM.Application.Command.CommandDTO;
using FM.Application.Command.CommandDTO.UserCommandDTO;

namespace FM.Application.Interfaces.ICommand
{
    public interface IUserCommand
    {
        public Task CreateUserAsync(CreateUserCommandDTO command);
        public Task UpdateUserAsync(UpdateUserCommandDTO command);
        public Task DeleteUserAsync(DeleteUserCommandDTO command);
    }
}
