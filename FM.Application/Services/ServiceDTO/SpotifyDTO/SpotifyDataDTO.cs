using System.Text.Json.Serialization;


namespace FM.Application.Services.ServiceDTO.SpotifyDTO
{
    public class SpotifyDataDTO
    {
        public string SongName {get ; set;}
        public string Artist { get; set; }
        public string Album { get; set; }
        public string AlbumImageUrl { get; set; }
        public string SongUrl { get; set; }
        public int Rank { get; set; }

        [JsonIgnore]
        public TimeSpan Progress { get; set; }

        [JsonIgnore]
        public TimeSpan Duration { get; set; }
        public bool isPlaying { get; set; }

        public string FormattedProgress => $"{Progress.Minutes:D2}:{Progress.Seconds:D2}";
        public string FormattedDuration => $"{Duration.Minutes:D2}:{Duration.Seconds:D2}";
    }
}
