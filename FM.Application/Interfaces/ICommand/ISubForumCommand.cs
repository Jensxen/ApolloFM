using FM.Application.Command.CommandDTO;
using FM.Application.Command.CommandDTO.SubForumCommandDTO;

namespace FM.Application.Interfaces.ICommand
{
    public interface ISubForumCommand
    {
        Task CreateSubForumAsync(CreateSubForumCommandDTO command);
        Task UpdateSubForumAsync(UpdateSubForumCommandDTO command);
        Task DeleteSubForumAsync(DeleteSubForumCommandDTO command);
    }
}
