using FM.Application.Command.CommandDTO;
using FM.Application.Command.CommandDTO.SubForumCommandDTO;

namespace FM.Application.Interfaces.ICommand
{
    public interface ISubForumCommand
    {
        public Task CreateSubForumAsync(CreateSubForumCommandDTO command);
        public Task UpdateSubForumAsync(UpdateSubForumCommandDTO command);
        public Task DeleteSubForumAsync(DeleteSubForumCommandDTO command);
    }
}
