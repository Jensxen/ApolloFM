using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FM.Application.Command.CommandDTO.PostCommandDTO
{
    public class UpdatePostCommandDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string SpotifyPlaylistId { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
