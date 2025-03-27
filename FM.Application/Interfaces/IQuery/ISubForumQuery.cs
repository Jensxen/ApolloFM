using FM.Application.QueryDTO.SubForumDTO;

namespace FM.Application.Interfaces.IQuery
{
    public interface ISubForumQuery
    {
        Task<IEnumerable<SubForumQueryDTO>> GetAllSubForumsAsync();
        Task<SubForumQueryDTO> GetSubForumByIdAsync(int id);
    }
}