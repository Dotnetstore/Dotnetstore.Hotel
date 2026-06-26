using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnetstore.Hotel.Shared.Cqrs;

public static class ServiceCollectionExtensions
{
    private static readonly Type[] OpenHandlerTypes = [typeof(ICommandHandler<,>), typeof(IQueryHandler<,>)];

    public static IServiceCollection AddCqrs(this IServiceCollection services, params Assembly[] handlerAssemblies)
    {
        services.AddScoped<IDispatcher, Dispatcher>();

        foreach (var assembly in handlerAssemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                foreach (var implementedInterface in type.GetInterfaces())
                {
                    if (!implementedInterface.IsGenericType)
                    {
                        continue;
                    }

                    var openInterface = implementedInterface.GetGenericTypeDefinition();
                    if (OpenHandlerTypes.Contains(openInterface))
                    {
                        services.AddScoped(implementedInterface, type);
                    }
                }
            }
        }

        return services;
    }
}
