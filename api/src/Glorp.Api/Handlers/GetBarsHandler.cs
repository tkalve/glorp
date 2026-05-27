using Glorp.Api.Entities;
using Glorp.Api.Glorpiatr;

namespace Glorp.Api.Handlers;

public class GetBarsHandler : IRequestHandler<BarsRequest, BarsResponse>
{
    public async Task<BarsResponse> HandleAsync(
        BarsRequest request, 
        CancellationToken cancellationToken)
    {
        var bars = new List<Bar>() {
            new("BarOne", 60, 150.5),
            new("BarTwo", 72, 200.0),
            new("BarThree", 68, 180.3),
            new("BarFour", 75, 220.1),
            new("BarFive", 65, 160.0)
        };

        return new BarsResponse {
            Data = bars.Where(b => b.Height >= request.MinHeight),
            Success = true
        };
    }
}