using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FM.Domain.Entities
{
    public class UserRole
    {
        public int Id { get; protected set; } // Primary key
        public string Name { get; protected set; } // Role name (fks, "Moderator", "Staff", "HeadAdmin")
        public ICollection<User> Users { get; protected set; } = new List<User>();
        public ICollection<Permission> Permissions { get; protected set; } = new List<Permission>();

        [Timestamp]
        public byte[] RowVersion { get; protected set; }

        public UserRole() { }
        public void UpdateName(string name)
        {
            Name = name;
        }
    }

}