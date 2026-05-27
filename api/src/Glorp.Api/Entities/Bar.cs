using Glorp.Api.Glorpiatr;

namespace Glorp.Api.Entities;

/// <summary>
/// A Bar
/// </summary>
/// <param name="Name"></param>
/// <param name="Height"></param>
/// <param name="Weight"></param>
public record Bar(string Name, int Height, double Weight);

/// <summary>
/// A Glorp Bars request object, returns any Bar with a height greater than or equal to the provided MinHeight
/// </summary>
/// <param name="MinHeight"></param>
public record BarsRequest(int MinHeight) : IGlorpRequest<BarsResponse>;

/// <summary>
/// A Glorp Bars response object
/// </summary>
public class BarsResponse : IGlorpResponse<IEnumerable<Bar>>
{
    public IEnumerable<Bar>? Data { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
}
