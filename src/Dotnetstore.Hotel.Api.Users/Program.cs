using Dotnetstore.Hotel.Api.Users.Authentication;
using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Api.Users.Endpoints;
using Dotnetstore.Hotel.Api.Users.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<UsersDbContext>("identitydb");

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services
    .AddIdentityCore<ApplicationUser>()
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<UsersDbContext>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole(Roles.Administrator, Roles.Superuser));

builder.Services.AddCqrs(typeof(Program).Assembly);

var app = builder.Build();

await MigrateAndSeedAsync(app.Services);

app.MapDefaultEndpoints();
app.UseAuthentication();
app.UseAuthorization();
app.MapAuthEndpoints();
app.MapUserEndpoints();

app.Run();

static async Task MigrateAndSeedAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    await dbContext.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    foreach (var roleName in Roles.All)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new ApplicationRole { Id = Guid.NewGuid(), Name = roleName });
        }
    }

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    if (await userManager.FindByEmailAsync(SeedAdmin.Email) is null)
    {
        var admin = new ApplicationUser
        {
            Id = SeedAdmin.Id,
            Email = SeedAdmin.Email,
            UserName = SeedAdmin.Email,
            EmailConfirmed = true,
        };
        await userManager.CreateAsync(admin, SeedAdmin.Password);
        await userManager.AddToRoleAsync(admin, Roles.Administrator);
    }
}

internal static class SeedAdmin
{
    public static readonly Guid Id = Guid.Parse("99999999-9999-9999-9999-999999999999");
    public const string Email = "admin@dotnetstore.hotel";

    // Dev-only seed password, meets default ASP.NET Core Identity complexity rules.
    public const string Password = "Adm1n!2024";
}
