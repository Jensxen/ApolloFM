using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FM.Infrastructure.Database
{
    public class ApolloContextFactory : IDesignTimeDbContextFactory<ApolloContext>
    {
        public ApolloContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApolloContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Database=ApolloFM;Username=postgres;Password=admin");

            return new ApolloContext(optionsBuilder.Options);
        }
    }
}