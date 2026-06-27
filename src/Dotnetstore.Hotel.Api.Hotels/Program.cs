using Dotnetstore.Hotel.Api.Hotels.Authentication;
using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Endpoints;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HotelEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Hotel;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<HotelDbContext>("hoteldb");

builder.Services.AddCqrs(typeof(Program).Assembly);
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IEquipmentRepository, EquipmentRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

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

// Role names are duplicated literals (not shared with Api.Users) - the two services are separate
// deployables that only agree on the JWT contract, not on shared code.
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("administrator", "superuser"))
    .AddPolicy("FrontDeskOnly", policy => policy.RequireRole("administrator", "superuser", "desk"));

var app = builder.Build();

await MigrateAndSeedAsync(app.Services);

app.MapDefaultEndpoints();
app.UseAuthentication();
app.UseAuthorization();
app.MapHotelEndpoints();
app.MapEquipmentEndpoints();
app.MapRoomEndpoints();
app.MapTagEndpoints();
app.MapCustomerEndpoints();
app.MapBookingEndpoints();

app.Run();

static async Task MigrateAndSeedAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<HotelDbContext>();

    await dbContext.Database.MigrateAsync();

    if (!await dbContext.Hotels.AnyAsync())
    {
        var seedHotel = SeedHotel.Default;
        dbContext.Hotels.Add(seedHotel);
        await dbContext.SaveChangesAsync();
    }
}

internal static class SeedHotel
{
    public static readonly Guid Id = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static HotelEntity Default => HotelEntity.Create(
        Id,
        "Dotnetstore Grand Hotel",
        new Address("Glädjebacksgatan 4B", "Trelleborg", "23145", "Sweden"),
        new ContactInfo("+46 8 123 456", "info@dotnetstore.hotel", "https://dotnetstore.hotel"));
}
