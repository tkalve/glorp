namespace Glorp.Api.Glorpiatr;

public class Glorpiator(IServiceProvider serviceProvider) : IGlorpiator
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task<TResponse> SendAsync<TResponse>(
        IGlorpRequest<TResponse> request, 
        CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>)
            .MakeGenericType(requestType, typeof(TResponse));

        var handler = _serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException($"No handler registered for request type {requestType.Name}");

        var handleMethod = handlerType.GetMethod("HandleAsync")
            ?? throw new InvalidOperationException($"HandleAsync method not found on handler for {requestType.Name}");

        var resultTask = (Task<TResponse>)handleMethod.Invoke(handler, [request, cancellationToken])!;

        return await resultTask;
    }

    public async Task<object?> SendAsync(
        object request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var glorpInterface = requestType.GetInterfaces().FirstOrDefault(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IGlorpRequest<>))
            ?? throw new ArgumentException(
                $"Type {requestType.Name} does not implement IGlorpRequest<>", nameof(request));

        var responseType = glorpInterface.GetGenericArguments()[0];
        var sendMethod = typeof(Glorpiator)
            .GetMethods()
            .First(m => m.Name == nameof(SendAsync) && m.IsGenericMethod)
            .MakeGenericMethod(responseType);

        var task = (Task)sendMethod.Invoke(this, [request, cancellationToken])!;
        await task.ConfigureAwait(false);
        return task.GetType().GetProperty("Result")!.GetValue(task);
    }
}