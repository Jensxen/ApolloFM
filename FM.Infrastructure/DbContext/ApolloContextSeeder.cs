using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FM.Domain.Entities;

namespace FM.Infrastructure.Database
{
    // Infrastructure/Persistence/ForumContextSeeder.cs
    public static class ForumContextSeeder
    {
        public static void Seed(ApolloContext context)
        {
            if (!context.UserRoles.Any())
            {
                var roles = new List<UserRole>
                {
                    new UserRole { Name = "HeadAdmin" },
                    new UserRole { Name = "WebsiteStaff" },
                    new UserRole { Name = "ForumModerator" },
                    new UserRole { Name = "User" }
                };
                context.UserRoles.AddRange(roles);
            }

            if (!context.SubForums.Any())
            {
                var subForums = new List<SubForum>
                {
                    new SubForum { Name = "General Discussion", Description = "Talk about anything!" },
                    new SubForum { Name = "Music Recommendations", Description = "Share and discover new music." },
                    new SubForum { Name = "Technical Support", Description = "Get help with the forum or Spotify integration." }
                };
                context.SubForums.AddRange(subForums);
            }

            context.SaveChanges();
        }
    }
}