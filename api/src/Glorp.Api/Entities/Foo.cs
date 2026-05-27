using Glorp.Api.Glorpiatr;

namespace Glorp.Api.Entities;

/// <summary>
/// A Foo
/// </summary>
/// <param name="Name"></param>
/// <param name="Color"></param>
public record Foo(string Name, string Color);

/// <summary>
/// A Glorp Foos request object, returns any Foo with a name that contains the provided Name string (case-insensitive)
/// </summary>
/// <param name="Name"></param>
public record FoosRequest(string Name) : IGlorpRequest<FoosResponse>;

/// <summary>
/// A Glorp Foos response object
/// </summary>
public class FoosResponse : IGlorpResponse<IEnumerable<Foo>>
{
    public IEnumerable<Foo>? Data { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
}
