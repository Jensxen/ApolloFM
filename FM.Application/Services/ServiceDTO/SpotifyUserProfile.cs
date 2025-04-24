using System.Text.Json.Serialization;

namespace FM.Application.Services.ServiceDTO
{
    public class SpotifyUserProfile
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Uri { get; set; } = string.Empty;
        
        [JsonPropertyName("images")]
        public List<SpotifyImage>? Images { get; set; }
        
        public string? ProfileImageUrl => Images?.FirstOrDefault()?.Url;
        
        public SpotifyUserProfile()
        {
        }
        
        public SpotifyUserProfile(string id, string displayName, string email)
        {
            Id = id;
            DisplayName = displayName;
            Email = email;
        }
    }
    
    public class SpotifyImage
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
        
        [JsonPropertyName("height")]
        public int? Height { get; set; }
        
        [JsonPropertyName("width")]
        public int? Width { get; set; }
    }
}
