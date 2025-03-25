using FM.Application.Interfaces;
using FM.Infrastucture;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using ApolloFM;
using FM.Infrastructure.Repositories;
using FM.Infrastructure.Database;
using FM.Infrastucture.Repositories;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register HttpClient
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Retrieve the connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register DbContext (if using EF Core)
builder.Services.AddDbContextFactory<ApolloContext>(options =>
    options.UseNpgsql(connectionString));

// Register repositories and services


// Build and run the application
await builder.Build().RunAsync();