namespace Glorp.Api.Glorpiatr;

public interface IGlorpResponse<T>
{
    T? Data { get; set; }
    bool Success { get; set; }
    string? Message { get; set; }
};
