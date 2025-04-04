//using System.Collections.Generic;
//using System.Linq;
//using FM.Domain.Entities;

//namespace FM.Infrastructure.Database
//{
//    public static class ApolloContextSeeder
//    {
//        public static void Seed(ApolloContext context)
//        {
//            if (!context.UserRoles.Any())
//            {
//                var roles = new List<UserRole>
//                {
//                    new UserRole { Name = "User" },
//                    new UserRole { Name = "SubForum Moderator" },
//                    new UserRole { Name = "SubForum Admin" },
//                    new UserRole { Name = "Forum Admin" },
//                    new UserRole { Name = "Forum HeadAdmin" }
//                };
//                context.UserRoles.AddRange(roles);
//            }

//            if (!context.Permissions.Any())
//            {
//                var permissions = new List<Permission>
//                {
//                    new Permission { Name = Permissions.CreatePost },
//                    new Permission { Name = Permissions.EditPost },
//                    new Permission { Name = Permissions.DeletePost },
//                    new Permission { Name = Permissions.CreateComment },
//                    new Permission { Name = Permissions.EditComment },
//                    new Permission { Name = Permissions.DeleteComment },
//                    new Permission { Name = Permissions.CreateSubForum },
//                    new Permission { Name = Permissions.EditSubForum },
//                    new Permission { Name = Permissions.DeleteSubForum },
//                    new Permission { Name = Permissions.BanUserFromSubForum },
//                    new Permission { Name = Permissions.UnbanUserFromSubForum },
//                    new Permission { Name = Permissions.BanUserFromForum },
//                    new Permission { Name = Permissions.UnbanUserFromForum },
//                    new Permission { Name = Permissions.PromoteToSubForumModerator },
//                    new Permission { Name = Permissions.PromoteToForumAdmin },
//                    new Permission { Name = Permissions.DemoteToUser }
//                };
//                context.Permissions.AddRange(permissions);
//            }

//            if (!context.SubForums.Any())
//            {
//                var subForums = new List<SubForum>
//                {
//                    new SubForum { Name = "General Discussion", Description = "Talk about anything!" },
//                    new SubForum { Name = "Music Recommendations", Description = "Share and discover new music." },
//                    new SubForum { Name = "Technical Support", Description = "Get help with the forum or Spotify integration." }
//                };
//                context.SubForums.AddRange(subForums);
//            }

//            context.SaveChanges();

//            // Associate permissions with roles
//            var userRole = context.UserRoles.FirstOrDefault(r => r.Name == "User");
//            var subForumModeratorRole = context.UserRoles.FirstOrDefault(r => r.Name == "SubForum Moderator");
//            var subForumAdminRole = context.UserRoles.FirstOrDefault(r => r.Name == "SubForum Admin");
//            var forumAdminRole = context.UserRoles.FirstOrDefault(r => r.Name == "Forum Admin");
//            var forumHeadAdminRole = context.UserRoles.FirstOrDefault(r => r.Name == "Forum HeadAdmin");

//            if (userRole != null)
//            {
//                userRole.Permissions = new HashSet<Permission>(context.Permissions.Where(p =>
//                    p.Name == Permissions.CreatePost ||
//                    p.Name == Permissions.EditPost ||
//                    p.Name == Permissions.DeletePost ||
//                    p.Name == Permissions.CreateComment ||
//                    p.Name == Permissions.EditComment ||
//                    p.Name == Permissions.DeleteComment ||
//                    p.Name == Permissions.CreateSubForum ||
//                    p.Name == Permissions.EditSubForum ||
//                    p.Name == Permissions.DeleteSubForum));
//            }

//            if (subForumModeratorRole != null)
//            {
//                subForumModeratorRole.Permissions = new HashSet<Permission>(context.Permissions.Where(p =>
//                    p.Name == Permissions.BanUserFromSubForum ||
//                    p.Name == Permissions.DeletePost ||
//                    p.Name == Permissions.DeleteComment));
//                foreach (var permission in userRole.Permissions)
//                {
//                    subForumModeratorRole.Permissions.Add(permission);
//                }
//            }

//            if (subForumAdminRole != null)
//            {
//                subForumAdminRole.Permissions = new HashSet<Permission>(context.Permissions.Where(p =>
//                    p.Name == Permissions.BanUserFromSubForum ||
//                    p.Name == Permissions.DeletePost ||
//                    p.Name == Permissions.DeleteComment ||
//                    p.Name == Permissions.PromoteToSubForumModerator ||
//                    p.Name == Permissions.DemoteToUser ||
//                    p.Name == Permissions.DeleteSubForum));
//                foreach (var permission in subForumModeratorRole.Permissions)
//                {
//                    subForumAdminRole.Permissions.Add(permission);
//                }
//            }

//            if (forumAdminRole != null)
//            {
//                forumAdminRole.Permissions = new HashSet<Permission>(context.Permissions.Where(p =>
//                    p.Name == Permissions.BanUserFromForum ||
//                    p.Name == Permissions.DeleteSubForum));
//                foreach (var permission in userRole.Permissions)
//                {
//                    forumAdminRole.Permissions.Add(permission);
//                }
//            }

//            if (forumHeadAdminRole != null)
//            {
//                forumHeadAdminRole.Permissions = new HashSet<Permission>(context.Permissions.Where(p =>
//                    p.Name == Permissions.BanUserFromForum ||
//                    p.Name == Permissions.DeleteSubForum ||
//                    p.Name == Permissions.DemoteToUser ||
//                    p.Name == Permissions.PromoteToForumAdmin));
//                foreach (var permission in forumAdminRole.Permissions)
//                {
//                    forumHeadAdminRole.Permissions.Add(permission);
//                }
//            }

//            context.SaveChanges();
//        }
//    }
//}