using ApolloFM;
using FM.Infrastructure.Database;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add your DbContext
builder.Services.AddDbContext<ApolloContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Build the host
var host = builder.Build();

// Seed the database
//using (var scope = host.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    var context = services.GetRequiredService<ApolloContext>();
//    context.Database.Migrate();
//    ApolloContextSeeder.Seed(context);
//}

await host.RunAsync();
