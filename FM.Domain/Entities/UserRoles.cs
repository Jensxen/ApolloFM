using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FM.Domain.Entities
{
    public class UserRole
    {
        public int Id { get; set; } // Primary key
        public string Name { get; set; } // Role name (fks, "Moderator", "Staff", "HeadAdmin")
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }

}