using Glorp.Api.Entities;
using Glorp.Api.Glorpiatr;

namespace Glorp.Api.Handlers;

public class GetBarsHandler : IRequestHandler<GetBarsHandler.GetBarsRequest, GetBarsHandler.GetBarsResponse>
{
    /// <summary>
    /// A Glorp Bars request object, returns any Bar with a height greater than or equal to the provided MinHeight
    /// </summary>
    /// <param name="MinHeight"></param>
    public record GetBarsRequest(int MinHeight) : IGlorpRequest<GetBarsResponse>;

    /// <summary>
    /// A Glorp Bars response object
    /// </summary>
    public class GetBarsResponse : IGlorpResponse<IEnumerable<Bar>>
    {
        public IEnumerable<Bar>? Data { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    public async Task<GetBarsResponse> HandleAsync(
        GetBarsRequest request,
        CancellationToken cancellationToken)
    {
        var bars = new List<Bar>() {
            new("BarOne", 60, 150.5),
            new("BarTwo", 72, 200.0),
            new("BarThree", 68, 180.3),
            new("BarFour", 75, 220.1),
            new("BarFive", 65, 160.0)
        };

        return new GetBarsResponse {
            Data = bars.Where(b => b.Height >= request.MinHeight),
            Success = true
        };
    }
}
