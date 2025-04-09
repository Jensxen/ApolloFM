using FM.Application.Command.CommandDTO;
using FM.Application.Command.CommandDTO.PostCommandDTO;

namespace FM.Application.Interfaces.ICommand
{
    public interface IPostCommand
    {
       public Task CreatePostAsync(CreatePostCommandDTO command);
       public Task UpdatePostAsync(UpdatePostCommandDTO command);
       public Task DeletePostAsync(DeletePostCommandDTO command);
    }
}
