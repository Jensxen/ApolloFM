using FM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FM.Infrastructure.Database
{
    public class ApolloContext : DbContext
    {
        public ApolloContext(DbContextOptions<ApolloContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<SubForum> SubForums { get; set; }

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

            // Configure Post-Comment relationship
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId);

            // Configure User-Comment relationship
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
