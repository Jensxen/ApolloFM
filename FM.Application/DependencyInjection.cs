using Microsoft.Extensions.DependencyInjection;
using FM.Application.Interfaces.ICommand;
using FM.Application.Command;
using FM.Application.Interfaces.IQuery;
using FM.Application.Query;

namespace FM.Application
{
    public static class ApplicationDependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register the command interfaces
            services.AddScoped<ISubForumCommand, SubForumCommand>();
            services.AddScoped<IUserCommand, UserCommand>();
            services.AddScoped<IPostCommand, PostCommand>();

            // Register the query interfaces
            services.AddScoped<ISubForumQuery, SubForumQuery>();

            return services;
        }
    }
}


