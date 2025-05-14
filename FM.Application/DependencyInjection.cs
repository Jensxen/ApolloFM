using Microsoft.Extensions.DependencyInjection;
using FM.Application.Interfaces.ICommand;
using FM.Application.Command;
using FM.Application.Interfaces.IQuery;
using FM.Application.Query;
using FM.Application.Interfaces.IRepositories;
using FM.Domain.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication;
using FM.Application.Services.SpotifyServices;
using FM.Application.Services.ForumServices;
using FM.Application.Services.UserServices;
using FM.Application.Services.ServiceDTO.UserDTO;

namespace FM.Application
{
    public static class ApplicationDependencyInjection
    {
        public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        bool isApiContext = false,
        bool registerAuthenticationProvider = true)
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
            services.AddScoped<IForumService, ForumService>();


            if (isApiContext)
            {
                services.AddHttpContextAccessor();
                services.AddScoped<IUserService, ServerUserService>();
            }
            else
            {
                services.AddScoped<IUserService, UserService>();
            }

            if (!isApiContext)
            {
                if (registerAuthenticationProvider)
                {
                    services.AddScoped<SpotifyAuthenticationStateProvider>();
                    services.AddScoped<AuthenticationStateProvider>(provider =>
                        provider.GetRequiredService<SpotifyAuthenticationStateProvider>());
                }
            }

            return services;
        }
    }
}


