public class SpotifyUserProfile
{
    public string DisplayName { get; set; }
    public string Email { get; set; }
    public string Id { get; set; }
    public List<SpotifyImage> Images { get; set; } = new List<SpotifyImage>();

    private string _defaultProfilePicUrl = "images/default-profile.jpg";
    public string ProfilePictureUrl =>
        Images?.FirstOrDefault()?.Url ?? _defaultProfilePicUrl;
}


public class SpotifyImage
{
    public int? Height { get; set; }
    public int? Width { get; set; }
    public string Url { get; set; }
}
