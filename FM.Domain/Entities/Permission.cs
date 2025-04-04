

using System.ComponentModel.DataAnnotations;

namespace FM.Domain.Entities
{
    public class Permission
    {
        public int Id {get; protected set;}
        public string Name { get; protected set; }
        public ICollection<UserRole> UserRoles { get; protected set; } = new List<UserRole>();

        [Timestamp]
        public byte[] RowVersion { get; protected set; }

        public void UpdateName(string name)
        {
            Name = name;
        }
    }
}
