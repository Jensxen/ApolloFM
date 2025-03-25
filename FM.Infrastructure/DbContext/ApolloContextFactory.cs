using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FM.Infrastructure.Database
{
    public class ApolloContextFactory : IDesignTimeDbContextFactory<ApolloContext>
    {
        public ApolloContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApolloContext>();
            optionsBuilder.UseSqlServer("Server=localhost;Database=ApolloDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");

            return new ApolloContext(optionsBuilder.Options);
        }
    }
}



