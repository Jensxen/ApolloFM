using ApolloFM;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using FM.Application;
using FM.Application.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Add auth to the application
builder.Services.AddAuthorizationCore();

// Add OIDC authentication
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("OidcProviderOptions", options.ProviderOptions);
    options.ProviderOptions.Authority = "https://accounts.spotify.com";
    options.ProviderOptions.ClientId = "824cb0e5e1d549c683c642d9c9ae062b";
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.DefaultScopes.Add("user-read-email");
    options.ProviderOptions.DefaultScopes.Add("user-read-private");
    options.ProviderOptions.DefaultScopes.Add("user-top-read");
    options.ProviderOptions.DefaultScopes.Add("user-read-currently-playing");
    options.ProviderOptions.RedirectUri = builder.HostEnvironment.BaseAddress + "authentication/login-callback";
    options.UserOptions.RoleClaim = "role"; // Tilføj dette, hvis du bruger roller
});

// Register AuthorizationMessageHandler
builder.Services.AddScoped<AuthorizationMessageHandler>();

// Configure HttpClient for Spotify API
builder.Services.AddHttpClient("SpotifyAPI", client =>
{
    client.BaseAddress = new Uri("https://api.spotify.com/v1/");
}).AddHttpMessageHandler(sp =>
    sp.GetRequiredService<AuthorizationMessageHandler>()
        .ConfigureHandler(
            authorizedUrls: new[] { "https://api.spotify.com/v1/" },
            scopes: new[] { "user-read-email", "user-read-private", "user-top-read", "user-read-currently-playing" }
        ));

// Configure HttpClient for your own API
builder.Services.AddHttpClient("ApolloAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7043/"); // Your API URL
});

// Add default HttpClient (pointing to the host)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register custom AuthenticationStateProvider
builder.Services.AddScoped<SpotifyAuthenticationStateProvider>(sp =>
{
    var jsRuntime = sp.GetRequiredService<IJSRuntime>();
    var nav = sp.GetRequiredService<NavigationManager>();
    return new SpotifyAuthenticationStateProvider(jsRuntime, nav);
});

builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<SpotifyAuthenticationStateProvider>());

builder.Services.AddScoped<TokenService>();

builder.Services.AddApplicationServices(isApiContext: false, registerAuthenticationProvider: false);

builder.Services.AddScoped<AuthService>(provider =>
{
    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    var authStateProvider = provider.GetRequiredService<AuthenticationStateProvider>();
    var navigationManager = provider.GetRequiredService<NavigationManager>();
    var tokenService = provider.GetRequiredService<TokenService>();
    
    return new AuthService(
        httpClientFactory, 
        authStateProvider, 
        navigationManager, 
        tokenService);
});


// Build and run the host
var host = builder.Build();
await host.RunAsync();
