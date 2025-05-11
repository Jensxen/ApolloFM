// FM.Infrastructure/Repositories/ForumRepositories/ForumRepository.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FM.Application.Interfaces.IRepositories;
using FM.Application.Interfaces.IRepositories;
using FM.Application.Services.ServiceDTO;
using FM.Domain.Entities;
using FM.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace FM.Infrastructure.Repositories.ForumRepositories
{
    public class ForumRepository : IForumRepository
    {
        private readonly ApolloContext _context;

        public ForumRepository(ApolloContext context)
        {
            _context = context;
        }

        public async Task<List<ForumTopicDto>> GetAllTopicsAsync()
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.SubForum)
                .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return posts.Select(p => new ForumTopicDto
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                AuthorId = p.UserId,
                AuthorName = p.User?.DisplayName ?? "Anonymous",
                AuthorProfileImage = null, // Add profile image logic if available
                CreatedAt = p.CreatedAt,
                LastUpdatedAt = p.CreatedAt, // Assuming no update tracking
                CommentCount = p.Comments?.Count ?? 0,
                SubForumId = p.SubForumId,
                SubForumName = p.SubForum?.Name ?? "General",
                Icon = DetermineIconForPost(p),
                Comments = null // Don't include comments in the list view
            }).ToList();
        }

        public async Task<ForumTopicDto> GetTopicByIdAsync(int id)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.SubForum)
                .Include(p => p.Comments.OrderByDescending(c => c.CreatedAt))
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return null;
            }

            return new ForumTopicDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                AuthorId = post.UserId,
                AuthorName = post.User?.DisplayName ?? "Anonymous",
                AuthorProfileImage = null, // Add profile image logic if available
                CreatedAt = post.CreatedAt,
                LastUpdatedAt = post.CreatedAt, // Assuming no update tracking
                CommentCount = post.Comments?.Count ?? 0,
                SubForumId = post.SubForumId,
                SubForumName = post.SubForum?.Name ?? "General",
                Icon = DetermineIconForPost(post),
                Comments = post.Comments?.Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    AuthorId = c.UserId,
                    AuthorName = c.User?.DisplayName ?? "Anonymous",
                    AuthorProfileImage = null, // Add profile image logic if available
                    CreatedAt = c.CreatedAt
                }).ToList()
            };
        }

        public async Task<List<SubForumDto>> GetAllSubForumsAsync()
        {
            var subforums = await _context.SubForums.ToListAsync();

            return subforums.Select(sf => new SubForumDto
            {
                Id = sf.Id,
                Name = sf.Name,
                Description = sf.Description
            }).ToList();
        }

        public async Task<ForumTopicDto> CreateTopicAsync(CreateTopicDto createTopicDto, string userId)
        {
            // Validate subforum exists
            var subForum = await _context.SubForums.FindAsync(createTopicDto.SubForumId);
            if (subForum == null)
            {
                // If specified subforum doesn't exist, use default (first one)
                subForum = await _context.SubForums.FirstOrDefaultAsync();
                if (subForum == null)
                {
                    throw new InvalidOperationException("No valid subforum found.");
                }
                createTopicDto.SubForumId = subForum.Id;
            }

            // Create post
            var post = new Post(
                createTopicDto.Title,
                createTopicDto.Content,
                null, // No Spotify playlist
                userId,
                createTopicDto.SubForumId
            );

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Get the created post with related data
            var createdPost = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.SubForum)
                .FirstOrDefaultAsync(p => p.Id == post.Id);

            // Map to DTO
            return new ForumTopicDto
            {
                Id = createdPost.Id,
                Title = createdPost.Title,
                Content = createdPost.Content,
                AuthorId = createdPost.UserId,
                AuthorName = createdPost.User?.DisplayName ?? "Anonymous",
                CreatedAt = createdPost.CreatedAt,
                LastUpdatedAt = createdPost.CreatedAt,
                CommentCount = 0,
                SubForumId = createdPost.SubForumId,
                SubForumName = createdPost.SubForum?.Name ?? "General",
                Icon = createTopicDto.Icon ?? "fas fa-comments",
                Comments = new List<CommentDto>()
            };
        }

        public async Task<CommentDto> AddCommentAsync(AddCommentDto addCommentDto, string userId)
        {
            // Validate post exists
            var post = await _context.Posts.FindAsync(addCommentDto.PostId);
            if (post == null)
            {
                throw new InvalidOperationException($"Post with ID {addCommentDto.PostId} not found.");
            }

            // Create comment
            var comment = new Comment(
                addCommentDto.Content,
                userId,
                addCommentDto.PostId
            );

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Get the created comment with user data
            var createdComment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            // Map to DTO
            return new CommentDto
            {
                Id = createdComment.Id,
                Content = createdComment.Content,
                AuthorId = createdComment.UserId,
                AuthorName = createdComment.User?.DisplayName ?? "Anonymous",
                AuthorProfileImage = null, // Add profile image logic if available
                CreatedAt = createdComment.CreatedAt
            };
        }

        private string DetermineIconForPost(Post post)
        {
            // Determine an icon based on post content
            if (post.Title.Contains("favorite", StringComparison.OrdinalIgnoreCase) ||
                post.Title.Contains("best", StringComparison.OrdinalIgnoreCase))
                return "fas fa-star";

            if (post.Title.Contains("album", StringComparison.OrdinalIgnoreCase) ||
                post.Title.Contains("vinyl", StringComparison.OrdinalIgnoreCase))
                return "fas fa-record-vinyl";

            if (post.Title.Contains("concert", StringComparison.OrdinalIgnoreCase) ||
                post.Title.Contains("live", StringComparison.OrdinalIgnoreCase))
                return "fas fa-microphone";

            if (post.Title.Contains("recommend", StringComparison.OrdinalIgnoreCase))
                return "fas fa-headphones";

            return "fas fa-comments";
        }
    }
}
