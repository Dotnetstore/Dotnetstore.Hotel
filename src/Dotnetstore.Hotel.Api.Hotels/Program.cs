using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Endpoints;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;
using HotelEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Hotel;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<HotelDbContext>("hoteldb");

builder.Services.AddCqrs(typeof(Program).Assembly);
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

await MigrateAndSeedAsync(app.Services);

app.MapDefaultEndpoints();
app.MapHotelEndpoints();

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
