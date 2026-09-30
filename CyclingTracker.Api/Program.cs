using CyclingTracker.Api.Data;
using CyclingTracker.Api.Models;
using FitFileParsing;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
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

builder.Services.AddScoped<IFitImportService, FitImportService>();
builder.Services.AddScoped<IFitFileParser, FitFileParser>();

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

builder.Services.ConfigureApplicationCookie(options =>
{
    // Prevent the 404 redirect loop. Force a clean 401 instead so you can see if auth fails.
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };

    // 👈 CRITICAL: Allows the cookie to be sent across different ports (localhost:3000 -> 5206)
    options.Cookie.SameSite = SameSiteMode.None; 
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // SameSite=None requires Secure
});

builder.Services.Configure<CookieAuthenticationOptions>(IdentityConstants.ExternalScheme, options =>
{
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// 🟢 ADD THIS: Configure the Two-Factor cookie to allow cross-origin deletion
builder.Services.Configure<CookieAuthenticationOptions>(IdentityConstants.TwoFactorUserIdScheme, options =>
{
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
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
