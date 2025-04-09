using Microsoft.Extensions.DependencyInjection;
using FM.Application.Interfaces.ICommand;
using FM.Application.Command;
using FM.Application.Interfaces.IQuery;
using FM.Application.Query;
using FM.Application.Interfaces.IRepositories;
using FM.Domain.Entities;
using FM.Application.Services;

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
            services.AddScoped<ICommentCommand, CommentCommand>();

            // Register the query interfaces
            services.AddScoped<ISubForumQuery, SubForumQuery>();
            services.AddScoped<IPostQuery, PostQuery>();
            services.AddScoped<IUserQuery, UserQuery>();

            // Services
            services.AddScoped<SpotifyService>();

            return services;
        }
    }
}


