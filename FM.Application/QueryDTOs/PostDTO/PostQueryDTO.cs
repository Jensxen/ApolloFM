using FM.Application.QueryDTO.CommentDTO;
namespace FM.Application.QueryDTO.PostDTO
{
    public class PostQueryDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string SpotifyPlaylistId { get; set; }
        public string UserId { get; set; }
        public int SubForumId { get; set; }

        // Include comments
        public IEnumerable<CommentQueryDTO> Comments { get; set; }
    }

    public class PostCreateDTO
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string SpotifyPlaylistId { get; set; }
        public string UserId { get; set; }
        public int SubForumId { get; set; }
    }

    public class PostUpdateDTO
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string SpotifyPlaylistId { get; set; }
    }
}