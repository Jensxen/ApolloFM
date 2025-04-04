using FM.Application.QueryDTO.UserDTO;

namespace FM.Application.Interfaces.IQuery
{
    public interface IUserQuery
    {
        //Task<IEnumerable<UserQueryDTO>> GetAllUsersAsync();
        Task<UserQueryDTO> GetUserByIdAsync(string id);
    }
}
