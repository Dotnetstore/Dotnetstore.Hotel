using Dotnetstore.Hotel.Shared.Sdk.Client;
using Dotnetstore.Hotel.Shared.Sdk.Client.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnetstore.Hotel.Shared.Sdk;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IHotelClient"/> against the given base address (default: the Aspire-discoverable
    /// "apihotels" resource). The "https+http://" scheme requires the consumer to have service discovery
    /// configured (e.g. via ServiceDefaults' <c>AddServiceDefaults()</c>) for the address to resolve.
    /// </summary>
    public static IServiceCollection AddHotelSdk(this IServiceCollection services, string baseAddress = "https+http://apihotels")
    {
        services.AddHttpClient<IHotelClient, HotelClient>(client => client.BaseAddress = new Uri(baseAddress));
        return services;
    }

    /// <summary>
    /// Registers <see cref="IUserClient"/> against the given base address (default: the Aspire-discoverable
    /// "apiusers" resource). The "https+http://" scheme requires the consumer to have service discovery
    /// configured (e.g. via ServiceDefaults' <c>AddServiceDefaults()</c>) for the address to resolve.
    /// </summary>
    public static IServiceCollection AddUserSdk(this IServiceCollection services, string baseAddress = "https+http://apiusers")
    {
        services.AddHttpClient<IUserClient, UserClient>(client => client.BaseAddress = new Uri(baseAddress));
        return services;
    }
}
