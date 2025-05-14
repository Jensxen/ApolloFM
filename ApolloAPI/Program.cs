using FM.Infrastructure;
using FM.Application;
using Microsoft.AspNetCore.Components.WebAssembly.Server;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.Cookies;
using AspNet.Security.OAuth.Spotify;
using Microsoft.EntityFrameworkCore;
using FM.Infrastructure.Database;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication;
using FM.Application.Services.SpotifyServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<SpotifyService>();
builder.Services.AddSingleton<TokenService>();

builder.Services.AddTransient<SpotifyTokenHandler>();
builder.Services.AddTransient<ApiSpotifyAuthorizationMessageHandler>();

builder.Services.AddHttpClient("SpotifyAPIAuth", client =>
{
    client.BaseAddress = new Uri("https://api.spotify.com/v1/");
}).AddHttpMessageHandler<SpotifyTokenHandler>();

builder.Services.AddHttpClient("ApolloAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7043/"); 
}).AddHttpMessageHandler<ApiSpotifyAuthorizationMessageHandler>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp",
        policy => policy
            .WithOrigins("https://localhost:7210")  // Your Blazor WebAssembly URL
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());  // This is important for credentials
});

builder.Services.PostConfigure<OAuthOptions>(SpotifyAuthenticationDefaults.AuthenticationScheme, options =>
{
    var provider = builder.Services.BuildServiceProvider();
    options.StateDataFormat = provider.GetRequiredService<ISecureDataFormat<AuthenticationProperties>>();
});

builder.Services.AddAuthorization();

// Register DbContext
builder.Services.AddDbContext<ApolloContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register HttpClient for SpotifyService
builder.Services.AddHttpClient<SpotifyService>(client =>
{
    client.BaseAddress = new Uri("https://api.spotify.com/v1/");
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.None; // For cross-origin requests
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApplicationServices(isApiContext: true);

builder.Services.AddDataProtection();
builder.Services.AddScoped<ISecureDataFormat<AuthenticationProperties>, CustomSecureDataFormat>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSession();
app.UseRouting();
app.UseCors("AllowBlazorApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
