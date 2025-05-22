using FM.Domain.Entities;
using System.ComponentModel.DataAnnotations;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }

    public string UserId { get; set; }
    public User User { get; set; }

    public int PostId { get; set; }
    public Post Post { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; }

    public Comment() { } 
    public Comment(string content, string userId, int postId)
    {
        Content = content;
        UserId = userId;
        PostId = postId;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateContent(string content)
    {
        Content = content;
    }
}
