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


builder.Services.AddDistributedMemoryCache(); // Kræves til sessionstorage
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // Hvor længe en sessions varer
    options.Cookie.HttpOnly = true; // God sikkerhedspraksis
    options.Cookie.IsEssential = true; // Markerer cookie som essentiel for GDPR
    options.Cookie.SameSite = SameSiteMode.None; // For cross-origin requests
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Brug kun over HTTPS
});




// Single authentication configuration
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = SpotifyAuthenticationDefaults.AuthenticationScheme;
//})
//.AddCookie(options =>
//{
//    options.Cookie.HttpOnly = true;
//    options.Cookie.SameSite = SameSiteMode.None; //cross-site cookies
//    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

//    options.ExpireTimeSpan = TimeSpan.FromHours(12);
//    options.SlidingExpiration = true;

//    options.LoginPath = "/api/auth/login";
//    options.AccessDeniedPath = "/api/auth/access-denied";
//})

//.AddSpotify(options =>
//{
//    options.ClientId = builder.Configuration["Spotify:ClientId"];
//    options.ClientSecret = builder.Configuration["Spotify:ClientSecret"];
//    options.SaveTokens = true;
//    options.CallbackPath = "/api/auth/callback";

//    // Scope konfiguration
//    options.Scope.Add("user-top-read");
//    options.Scope.Add("user-read-private");
//    options.Scope.Add("user-read-email");
//    options.Scope.Add("playlist-read-private");
//    options.Scope.Add("user-read-playback-state");
//    options.Scope.Add("user-read-currently-playing");

//    // Correlation Cookie konfiguration
//    options.CorrelationCookie = new CookieBuilder
//    {
//        HttpOnly = true,
//        SameSite = SameSiteMode.None,
//        SecurePolicy = CookieSecurePolicy.Always,
//        IsEssential = true
//    };
//    options.Events = new OAuthEvents
//    {
//        OnRemoteFailure = context =>
//        {
//            Console.WriteLine($"OAuth Remote Failure: {context.Failure?.Message}");

//            // Hvis der er en state-fejl, log den og fortsæt
//            if (context.Failure?.Message?.Contains("oauth state") == true)
//            {
//                Console.WriteLine("OAuth State error detected, continuing with authentication");
//                context.HandleResponse();
//                context.Response.Redirect("/api/auth/handle-state-error");
//                return Task.CompletedTask;
//            }

//            return Task.CompletedTask;
//        },
//        OnTicketReceived = context =>
//        {
//            Console.WriteLine("OAuth ticket received successfully!");
//            return Task.CompletedTask;
//        },
//        OnCreatingTicket = context =>
//        {
//            Console.WriteLine($"Creating OAuth ticket for user: {context.Identity?.Name ?? "Unknown"}");
//            return Task.CompletedTask;
//        },
//        OnRedirectToAuthorizationEndpoint = context =>
//        {
//            Console.WriteLine($"Redirecting to authorization endpoint: {context.RedirectUri}");
//            context.Response.Redirect(context.RedirectUri);
//            return Task.CompletedTask;
//        }
//    };
//})
//.AddJwtBearer(options =>
//{
//    options.Authority = "https://accounts.spotify.com";
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateAudience = false,
//        ValidateIssuer = false,
//    };
//});

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
