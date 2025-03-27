using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FM.Application.Command.CommandDTO.UserCommandDTO
{
    public class UpdateUserCommandDTO
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string SpotifyUserId { get; set; }
        public int UserRoleId { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
