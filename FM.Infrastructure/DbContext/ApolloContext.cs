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
        public DbSet<Permission> Permissions { get; set; }

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
                .HasForeignKey(p => p.SubForumId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Post-Comment relationship with Cascade Delete
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade); // Enable cascade delete

            // Configure User-Comment relationship
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure UserRole-Permission relationship
            modelBuilder.Entity<UserRole>()
                .HasMany(r => r.Permissions)
                .WithMany(p => p.UserRoles)
                .UsingEntity(j => j.ToTable("UserRolePermissions"));

            // Configure RowVersion as a concurrency token
            modelBuilder.Entity<User>()
                .Property(u => u.RowVersion)
                .IsRowVersion()
                .ValueGeneratedOnAddOrUpdate();

            modelBuilder.Entity<Post>()
                .Property(p => p.RowVersion)
                .IsRowVersion()
                .ValueGeneratedOnAddOrUpdate();

            modelBuilder.Entity<Comment>()
                .Property(c => c.RowVersion)
                .IsRowVersion()
                .ValueGeneratedOnAddOrUpdate();

            modelBuilder.Entity<SubForum>()
                .Property(s => s.RowVersion)
                .IsRowVersion()
                .ValueGeneratedOnAddOrUpdate();

            modelBuilder.Entity<UserRole>()
                .Property(r => r.RowVersion)
                .IsRowVersion()
                .ValueGeneratedOnAddOrUpdate();

            modelBuilder.Entity<Permission>()
                .Property(p => p.RowVersion)
                .IsRowVersion()
                .ValueGeneratedOnAddOrUpdate();

            base.OnModelCreating(modelBuilder);
        }
    }
}


