using FM.Application.Command.CommandDTO;
using FM.Application.Command.CommandDTO.SubForumCommandDTO;

namespace FM.Application.Interfaces.ICommand
{
    public interface ISubForumCommand
    {
        void CreateSubForum(CreateSubForumCommandDTO command);
        void UpdateSubForum(UpdateSubForumCommandDTO command);
        void DeleteSubForum(DeleteSubForumCommandDTO command);
    }
}
