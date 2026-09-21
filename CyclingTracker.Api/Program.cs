using CyclingTracker.Api.Data;
using CyclingTracker.Api.Models;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services
    .AddIdentityApiEndpoints<Athlete>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// 2. Add CORS services and register your React port
const string AllowReactAppPolicy = "_allowReactApp";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowReactAppPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173") // <-- Match your exact active React port
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required if you use cookies or identity sessions
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
   app.UseHttpsRedirection(); 
}

//

app.UseCors(AllowReactAppPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("/api/auth").MapIdentityApi<Athlete>();
app.MapControllers();

app.Run();
