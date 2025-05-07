using Microsoft.Extensions.DependencyInjection;
using FM.Application.Interfaces.ICommand;
using FM.Application.Command;
using FM.Application.Interfaces.IQuery;
using FM.Application.Query;
using FM.Application.Interfaces.IRepositories;
using FM.Domain.Entities;
using FM.Application.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication;
using FM.Application.Interfaces.IServices;

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


            if (isApiContext)
            {
                // Only register ITokenService if it hasn't been registered already
                if (!services.Any(s => s.ServiceType == typeof(ITokenService)))
                {
                    services.AddScoped<ITokenService, ServerTokenService>();
                }
                services.AddScoped<IAuthService, ServerAuthService>();
            }
            else
            {
                // For Blazor, use browser storage implementation
                services.AddScoped<ITokenService, BrowserTokenService>();

                services.AddScoped<IAuthService, BrowserAuthService>();
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


