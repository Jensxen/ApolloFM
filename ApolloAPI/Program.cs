using FM.Domain.Interfaces;
using FM.Infrastructure.Repositories;
using FM.Infrastucture.DbContext; // Adjust the namespace to where your DbContext is located
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the DbContext
builder.Services.AddDbContext<ApolloContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); // Adjust the connection string name

// Register the repository
builder.Services.AddScoped<ISubForumRepository, SubForumRepository>(); // Adjust the implementation class name

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
