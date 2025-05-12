using ApolloFM;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using FM.Infrastructure;
using FM.Application;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ApolloFM.JsInterop;
using FM.Application.Services.AuthServices;
using FM.Application.Services.ForumServices;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Add auth to the application
builder.Services.AddAuthorizationCore();

// Configure HttpClient for Spotify API
builder.Services.AddHttpClient("SpotifyAPI", client =>
{
    client.BaseAddress = new Uri("https://api.spotify.com/v1/");
});

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

builder.Services.AddInfrastructure(builder.Configuration, isWebAssembly: true);

builder.Services.AddScoped<AuthService>(provider =>
{
    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    var authStateProvider = provider.GetRequiredService<AuthenticationStateProvider>();
    var navigationManager = provider.GetRequiredService<NavigationManager>();
    var tokenService = provider.GetRequiredService<TokenService>();
    var jsRuntime = provider.GetRequiredService<IJSRuntime>();

    return new AuthService(
        httpClientFactory, 
        authStateProvider, 
        navigationManager, 
        tokenService,
        jsRuntime);
});

// Add this line to set up the JS interop
builder.Services.AddScoped(sp => new AuthInteropService(
    sp.GetRequiredService<AuthenticationStateProvider>(),
    sp.GetRequiredService<NavigationManager>()
));

builder.Services.AddTransient<SpotifyTokenHandler>();
builder.Services.AddHttpClient("SpotifyAPIAuth", client =>
{
    client.BaseAddress = new Uri("https://api.spotify.com/v1/");
}).AddHttpMessageHandler<SpotifyTokenHandler>();


// Build and run the host
var host = builder.Build();
await host.RunAsync();
