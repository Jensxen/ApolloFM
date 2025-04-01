using FM.Application.Command.CommandDTO;
using FM.Application.Command.CommandDTO.PostCommandDTO;

namespace FM.Application.Interfaces.ICommand
{
    public interface IPostCommand
    {
        Task CreatePostAsync(CreatePostCommandDTO command);
        Task UpdatePostAsync(UpdatePostCommandDTO command);
        Task DeletePostAsync(DeletePostCommandDTO command);
    }
}
