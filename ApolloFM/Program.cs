using ApolloFM;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using FM.Application;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddHttpClient("SpotifyAPI", client =>
{
    client.BaseAddress = new Uri("https://api.spotify.com/v1/");
}).AddHttpMessageHandler(sp =>
    sp.GetRequiredService<AuthorizationMessageHandler>()
        .ConfigureHandler(
            authorizedUrls: new[] { "https://api.spotify.com/v1/" },
            scopes: new[] { "user-read-email", "user-read-private" }
        ));

builder.Services.AddOidcAuthentication(options =>
{
    options.ProviderOptions.Authority = "https://accounts.spotify.com";
    options.ProviderOptions.ClientId = "824cb0e5e1d549c683c642d9c9ae062b";
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.DefaultScopes.Add("user-read-email");
    options.ProviderOptions.DefaultScopes.Add("user-read-private");
    options.ProviderOptions.RedirectUri = builder.HostEnvironment.BaseAddress + "authentication/login-callback";
});


// In Blazor WebAssembly Program.cs
builder.Services.AddApplicationServices(); // Default is false


builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Build the host
var host = builder.Build();

await host.RunAsync();
