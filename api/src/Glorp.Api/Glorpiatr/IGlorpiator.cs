namespace Glorp.Api.Glorpiatr;

public interface IGlorpiator
{
    Task<TResponse> SendAsync<TResponse>(
        IGlorpRequest<TResponse> request, 
        CancellationToken cancellationToken = default);

    Task<object?> SendAsync(
        object request,
        CancellationToken cancellationToken = default);
}
