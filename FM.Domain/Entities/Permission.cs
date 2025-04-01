

namespace FM.Domain.Entities
{
    public class Permission
    {
        public int Id {get; set;}
        public string Name { get; set; }
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
