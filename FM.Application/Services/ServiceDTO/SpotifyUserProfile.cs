public class SpotifyUserProfile
{
    public string DisplayName { get; set; }
    public string Email { get; set; }
    public string Id { get; set; }
    public List<SpotifyImage> Images { get; set; }
    public string ProfilePictureUrl => Images?.FirstOrDefault()?.Url;
}

public class SpotifyImage
{
    public int? Height { get; set; }
    public int? Width { get; set; }
    public string Url { get; set; }
}
