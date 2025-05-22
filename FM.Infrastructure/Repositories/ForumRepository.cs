using FM.Application.Interfaces.IRepositories;
using FM.Domain.Entities.Enums;
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
            try
            {
                var topicIds = await _context.Posts
                    .Where(p => p.PostTypeId == (int)PostTypeEnum.Topic)
                    .Select(p => p.Id)
                    .ToListAsync();

                var topics = new List<ForumTopicDto>();

                foreach (var topicId in topicIds)
                {
                    try
                    {
                        var post = await _context.Posts.FindAsync(topicId);
                        if (post == null) continue;

                        var user = post.UserId != null ? await _context.Users.FindAsync(post.UserId) : null;
                        var subForum = post.SubForumId != 0 ? await _context.SubForums.FindAsync(post.SubForumId) : null;

                        var topic = new ForumTopicDto
                        {
                            Id = post.Id,
                            Title = post.Title ?? string.Empty,
                            Content = post.Content ?? string.Empty,
                            AuthorId = user?.Id ?? string.Empty,
                            AuthorName = user?.DisplayName ?? "Unknown User",
                            CreatedAt = post.CreatedAt,
                            LastUpdatedAt = post.UpdatedAt ?? post.CreatedAt,
                            SubForumId = post.SubForumId,
                            SubForumName = subForum?.Name ?? "General",
                            Icon = post.Icon ?? "fas fa-comments",
                            Comments = new List<CommentDto>()
                        };

                        topic.CommentCount = await _context.Comments.CountAsync(c => c.PostId == topicId);

                        var commentIds = await _context.Comments
                            .Where(c => c.PostId == topicId)
                            .Select(c => c.Id)
                            .ToListAsync();

                        foreach (var commentId in commentIds)
                        {
                            try
                            {
                                var comment = await _context.Comments.FindAsync(commentId);
                                if (comment == null) continue;

                                var commentUser = comment.UserId != null ?
                                    await _context.Users.FindAsync(comment.UserId) : null;

                                topic.Comments.Add(new CommentDto
                                {
                                    Id = comment.Id,
                                    Content = comment.Content ?? string.Empty,
                                    AuthorId = commentUser?.Id ?? string.Empty,
                                    AuthorName = commentUser?.DisplayName ?? "Anonymous",
                                    AuthorProfileImage = string.Empty,
                                    CreatedAt = comment.CreatedAt
                                });
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error processing comment {commentId}: {ex.Message}");
                            }
                        }

                        topics.Add(topic);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing topic {topicId}: {ex.Message}");
                    }
                }

                return topics.OrderByDescending(t => t.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllTopicsAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<ForumTopicDto> GetTopicByIdAsync(int id)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.SubForum)
                .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return null;
            }

            return new ForumTopicDto
            {
                Id = post.Id,
                Title = post.Title ?? string.Empty,
                Content = post.Content ?? string.Empty,
                AuthorId = post.User?.Id ?? string.Empty,
                AuthorName = post.User?.DisplayName ?? "Anonymous",
                AuthorProfileImage = null, 
                CreatedAt = post.CreatedAt,
                LastUpdatedAt = post.UpdatedAt ?? post.CreatedAt,
                CommentCount = post.Comments?.Count ?? 0,
                SubForumId = post.SubForumId,
                SubForumName = post.SubForum?.Name ?? "General",
                Icon = post.Icon ?? DetermineIconForPost(post),
                Comments = post.Comments?
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new CommentDto
                    {
                        Id = c.Id,
                        Content = c.Content ?? string.Empty,
                        AuthorId = c.User?.Id ?? string.Empty,
                        AuthorName = c.User?.DisplayName ?? "Anonymous",
                        AuthorProfileImage = null,
                        CreatedAt = c.CreatedAt
                    }).ToList() ?? new List<CommentDto>()
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
            var subForum = await _context.SubForums.FindAsync(createTopicDto.SubForumId);
            if (subForum == null)
            {
                subForum = await _context.SubForums.FirstOrDefaultAsync();
                if (subForum == null)
                {
                    throw new InvalidOperationException("No valid subforum found.");
                }
                createTopicDto.SubForumId = subForum.Id;
            }

            var title = createTopicDto.Title ?? string.Empty;
            var content = createTopicDto.Content ?? string.Empty;
            var icon = createTopicDto.Icon ?? "fas fa-comments";

            var post = new Post(
                title,
                content,
                createTopicDto.SpotifyPlaylistId,
                userId,
                createTopicDto.SubForumId
            );

            post.UpdateIcon(icon);

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            var createdPost = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.SubForum)
                .FirstOrDefaultAsync(p => p.Id == post.Id);

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
                Icon = createdPost.Icon ?? "fas fa-comments",
                Comments = new List<CommentDto>()
            };
        }

        public async Task<CommentDto> AddCommentAsync(AddCommentDto addCommentDto, string userId)
        {
            var post = await _context.Posts.FindAsync(addCommentDto.PostId);
            if (post == null)
            {
                throw new InvalidOperationException($"Post with ID {addCommentDto.PostId} not found.");
            }

            var comment = new Comment(
                addCommentDto.Content,
                userId,
                addCommentDto.PostId
            );

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var createdComment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            return new CommentDto
            {
                Id = createdComment.Id,
                Content = createdComment.Content,
                AuthorId = createdComment.UserId,
                AuthorName = createdComment.User?.DisplayName ?? "Anonymous",
                AuthorProfileImage = null, 
                CreatedAt = createdComment.CreatedAt
            };
        }

        private string DetermineIconForPost(Post post)
        {
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
