//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace FM.Application.Services.ServiceDTO
//{
//    public class SpotifyApiResponse
//    {
//        public SpotifyItem? Item { get; set; }
//        public bool IsPlaying { get; set; }
//        public int? ProgressMs { get; set; }
//    }

//    public class SpotifyItem
//    {
//        public string Name { get; set; }
//        public List<SpotifyArtist> Artists { get; set; }
//        public SpotifyAlbum Album { get; set; }
//        public ExternalUrls ExternalUrls { get; set; }
//        public int DurationMs { get; set; }
//    }

//    public class SpotifyArtist
//    {
//        public string Name { get; set; }
//    }

//    public class SpotifyAlbum
//    {
//        public string Name { get; set; }
//        public List<SpotifyImage> Images { get; set; }
//    }

//    public class SpotifyImage
//    {
//        public string Url { get; set; }
//    }

//    public class ExternalUrls
//    {
//        public string Spotify { get; set; }
//    }

//}
