using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnetstore.Hotel.Shared.Cqrs;

internal sealed class Dispatcher(IServiceProvider serviceProvider) : IDispatcher
{
    public Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
        => Invoke<TResult>(typeof(ICommandHandler<,>), command.GetType(), command, cancellationToken);

    public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
        => Invoke<TResult>(typeof(IQueryHandler<,>), query.GetType(), query, cancellationToken);

    private Task<TResult> Invoke<TResult>(Type openHandlerType, Type requestType, object request, CancellationToken cancellationToken)
    {
        var handlerType = openHandlerType.MakeGenericType(requestType, typeof(TResult));
        var handler = serviceProvider.GetRequiredService(handlerType);
        var method = handlerType.GetMethod("HandleAsync", BindingFlags.Public | BindingFlags.Instance)!;
        return (Task<TResult>)method.Invoke(handler, [request, cancellationToken])!;
    }
}
