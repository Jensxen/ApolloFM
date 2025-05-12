using FM.Application.Interfaces.IRepositories;
using FM.Application.Services.ServiceDTO;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace FM.Infrastructure.Repositories.WebAssembly
{
    public class WebForumRepository : IForumRepository
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public WebForumRepository(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApolloAPI");
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<List<ForumTopicDto>> GetTopicsAsync()
        {
            try
            {
                var topics = await _httpClient.GetFromJsonAsync<List<ForumTopicDto>>("api/forum/topics", _jsonOptions);
                return topics ?? new List<ForumTopicDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching topics: {ex.Message}");
                return new List<ForumTopicDto>();
            }
        }

        // Alias for GetTopicsAsync to satisfy the interface
        public Task<List<ForumTopicDto>> GetAllTopicsAsync()
        {
            return GetTopicsAsync();
        }

        public async Task<ForumTopicDto> GetTopicByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ForumTopicDto>($"api/forum/topics/{id}", _jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching topic {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<SubForumDto>> GetSubForumsAsync()
        {
            try
            {
                var subforums = await _httpClient.GetFromJsonAsync<List<SubForumDto>>("api/forum/subforums", _jsonOptions);
                return subforums ?? new List<SubForumDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching subforums: {ex.Message}");
                return new List<SubForumDto>();
            }
        }

        // Alias for GetSubForumsAsync to satisfy the interface
        public Task<List<SubForumDto>> GetAllSubForumsAsync()
        {
            return GetSubForumsAsync();
        }

        public async Task<ForumTopicDto> CreateTopicAsync(CreateTopicDto createTopicDto, string userId)
        {
            try
            {
                var topicRequest = new
                {
                    Title = createTopicDto.Title,
                    Content = createTopicDto.Content,
                    SubForumId = createTopicDto.SubForumId,
                    Icon = createTopicDto.Icon ?? "fas fa-comments",
                    SpotifyPlaylistId = "none" // Add a default value for SpotifyPlaylistId
                };

                // Log what we're sending in safe manner to prevent null reference errors
                Console.WriteLine($"Sending topic: Title={topicRequest.Title}, " +
                                  $"Content={topicRequest.Content?.Substring(0, Math.Min(20, topicRequest.Content?.Length ?? 0))}..., " +
                                  $"SubForumId={topicRequest.SubForumId}, " +
                                  $"Icon={topicRequest.Icon}");

                var response = await _httpClient.PostAsJsonAsync("api/forum/topics", topicRequest, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ForumTopicDto>(_jsonOptions);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error response from API: {errorContent}");
                    throw new InvalidOperationException($"Failed to create topic: {response.StatusCode}, {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating topic: {ex.Message}");
                throw;
            }
        }

        public async Task<CommentDto> AddCommentAsync(AddCommentDto addCommentDto, string userId)
        {
            try
            {
                // Again, using the DTO directly without wrapping it
                var commentRequest = new
                {
                    Content = addCommentDto.Content,
                    PostId = addCommentDto.PostId
                    // UserId is handled by the controller via claims
                };

                var response = await _httpClient.PostAsJsonAsync("api/forum/comments", commentRequest, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<CommentDto>(_jsonOptions);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error response from API: {errorContent}");
                    throw new InvalidOperationException($"Failed to add comment: {response.StatusCode}, {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding comment: {ex.Message}");
                throw;
            }
        }
    }
}
