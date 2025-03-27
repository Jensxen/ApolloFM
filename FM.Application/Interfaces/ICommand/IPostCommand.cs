using FM.Application.Command.CommandDTO;
using FM.Application.Command.CommandDTO.PostCommandDTO;

namespace FM.Application.Interfaces.ICommand
{
    public interface IPostCommand
    {
        void CreatePost(CreatePostCommandDTO command);
        void UpdatePost(UpdatePostCommandDTO command);
        void DeletePost(DeletePostCommandDTO command);
    }
}
