using Ascendly.Application.Interfaces;
using Ascendly.Infrastructure.Persistence;
using Ascendly.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Services
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//enabling controllers
builder.Services.AddControllers();

//registering the applicationdbcontext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//registering the auth service di
builder.Services.AddScoped<IAuthService, AuthService>();



var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//maps incoming HTTP requests to the appropriate controller actions
app.MapControllers();

app.Run();