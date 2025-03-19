using FM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FM.Infrastucture.DbContext
{
    public class ApolloContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<SubForum> SubForums { get; set; }

        public ApolloContext(DbContextOptions<ApolloContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure UserRole relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.UserRole)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.UserRoleId);

            // Configure SubForum relationship
            modelBuilder.Entity<Post>()
                .HasOne(p => p.SubForum)
                .WithMany(f => f.Posts)
                .HasForeignKey(p => p.SubForumId);

            base.OnModelCreating(modelBuilder);
        }
    }
}