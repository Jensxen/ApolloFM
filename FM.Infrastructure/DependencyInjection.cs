using FM.Infrastructure.Repositories;
using FM.Infrastructure.Database;
using FM.Infrastucture.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FM.Application.Interfaces;


namespace FM.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register the DbContext
            services.AddDbContext<ApolloContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Register the repositories
            services.AddScoped<ISubForumRepository, SubForumRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPostRepository, PostRepository>();

            // Register the UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
