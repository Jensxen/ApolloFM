using FM.Infrastructure;
using FM.Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.Cookies;
using AspNet.Security.OAuth.Spotify;
using Microsoft.EntityFrameworkCore;
using FM.Infrastructure.Database;
using Microsoft.AspNetCore.Authentication.OAuth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Single authentication configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = SpotifyAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None; // Required for cross-site cookies
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Requires HTTPS

    // Set a reasonable expiration time
    options.ExpireTimeSpan = TimeSpan.FromHours(12);
    options.SlidingExpiration = true;

    // Configure the login path and access denied path
    options.LoginPath = "/api/auth/login";
    options.AccessDeniedPath = "/api/auth/access-denied";
})
.AddSpotify(options =>
{
    options.ClientId = builder.Configuration["Spotify:ClientId"];
    options.ClientSecret = builder.Configuration["Spotify:ClientSecret"];
    options.CallbackPath = "/api/auth/callback"; // Update to match controller route

    options.Scope.Add("user-read-private");
    options.Scope.Add("user-read-email");
    options.Scope.Add("playlist-read-private");

    options.SaveTokens = true;

    // Add events to handle the authentication flow
    options.Events = new OAuthEvents
    {
        OnCreatingTicket = context =>
        {
            // Log successful authentication
            Console.WriteLine($"Creating ticket for user: {context.Identity.Name}");
            return Task.CompletedTask;
        },
        OnTicketReceived = context =>
        {
            // You can customize the ticket if needed
            return Task.CompletedTask;
        },
        OnRedirectToAuthorizationEndpoint = context =>
        {
            Console.WriteLine($"Redirecting to authorization endpoint: {context.RedirectUri}");
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        }
    };
})
.AddJwtBearer(options =>
{
    options.Authority = "https://accounts.spotify.com";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = false,
        ValidateIssuer = false,
    };
});

builder.Services.AddAuthorization();

// Register DbContext
builder.Services.AddDbContext<ApolloContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register HttpClient for SpotifyService
builder.Services.AddHttpClient<FM.Application.Services.SpotifyService>(client =>
{
    client.BaseAddress = new Uri("https://api.spotify.com/v1/");
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7210")  // Your Blazor app URL
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("Set-Cookie"); // Explicitly allow Cookie headers
    });
});



// Register infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Register application services
builder.Services.AddApplicationServices(isApiContext: true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseCors();

app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
