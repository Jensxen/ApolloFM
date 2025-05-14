// FM.Application/Services/ServiceDTO/ForumDTOs.cs
using System;
using System.Collections.Generic;

namespace FM.Application.Services.ServiceDTO
{
    public class ForumTopicDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string AuthorProfileImage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public int CommentCount { get; set; }
        public int SubForumId { get; set; }
        public string SubForumName { get; set; }
        public string Icon { get; set; }
        public List<CommentDto> Comments { get; set; }
    }

    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string AuthorProfileImage { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SubForumDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class CreateTopicDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public int SubForumId { get; set; }
        public string Icon { get; set; }
        public string UserId { get; set; }
    }

    public class AddCommentDto
    {
        public int PostId { get; set; }
        public string Content { get; set; }
    }
}
