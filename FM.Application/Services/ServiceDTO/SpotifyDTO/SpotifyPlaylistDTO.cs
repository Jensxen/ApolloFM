using System;
using System.Collections.Generic;

namespace FM.Application.Services.ServiceDTO.SpotifyDTO
{
    public class SpotifyPlaylistDTO
    {
        public string Id {get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int TracksCount { get; set; }
        public int SavesCount { get; set; }
        public bool IsOwner { get; set; }
        public string Owner { get; set; }
        public string ExternalUrl { get; set; }
    }
}
