namespace FM.Application.QueryDTO.CommentDTO
{
    public class CommentQueryDTO
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
        public int PostId { get; set; }
    }
}
