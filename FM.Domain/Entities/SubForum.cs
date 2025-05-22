using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FM.Domain.Entities
{
    public class SubForum
    {
        public int Id { get; protected set; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public ICollection<Post> Posts { get; protected set; } = new List<Post>();

        [Timestamp]
        public byte[] RowVersion { get; protected set; }

        public SubForum() { }

        public SubForum(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public void UpdateName(string name)
        {
            Name = name;
        }

        public void UpdateDescription(string description)
        {
            Description = description;
        }
    }

}