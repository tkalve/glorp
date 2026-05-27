namespace Glorp.Api.Glorpiatr;

public interface IRequestHandler<TRequest, TResponse> 
    where TRequest : IGlorpRequest<TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
}