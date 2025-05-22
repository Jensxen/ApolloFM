using FM.Application.Services.ForumServices;
using FM.Application.Interfaces.IRepositories;
using FM.Infrastructure.Repositories;
using FM.Infrastructure.Repositories.ForumRepositories;
using FM.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FM.Application.Interfaces;
using FM.Infrastucture.Repositories;
using FM.Infrastructure.Repositories.WebAssembly;

namespace FM.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, bool isWebAssembly = false)
        {
            if (isWebAssembly)
            {
                services.AddScoped<IForumRepository, WebForumRepository>();
                
                services.AddScoped<IForumService, ForumService>();
            }
            else
            {
                // Server-side
                
                // DbContext
                services.AddDbContext<ApolloContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

                // Repositories
                services.AddScoped<ISubForumRepository, SubForumRepository>();
                services.AddScoped<IUserRepository, UserRepository>();
                services.AddScoped<IPostRepository, PostRepository>();
                services.AddScoped<IUserRoleRepository, UserRoleRepository>();
                services.AddScoped<ICommentRepository, CommentRepository>();
                services.AddScoped<IForumRepository, ForumRepository>();

                services.AddScoped<IUnitOfWork, UnitOfWork>();
                
                services.AddScoped<IForumService, ForumService>();
            }
            
            return services;
        }
    }
}
