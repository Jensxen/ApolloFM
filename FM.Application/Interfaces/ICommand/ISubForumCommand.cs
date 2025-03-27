using FM.Application.Command.CommandDTO;
using FM.Application.Command.CommandDTO.SubForumCommandDTO;

namespace FM.Application.Interfaces.ICommand
{
    public interface ISubForumCommand
    {
        Task CreateSubForum(CreateSubForumCommandDTO command);
        Task UpdateSubForum(UpdateSubForumCommandDTO command);
        Task DeleteSubForum(DeleteSubForumCommandDTO command);
    }
}
