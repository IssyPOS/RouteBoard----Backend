namespace POSShopTicketing.Shared.Wrappers;

public class ApiResponse<T>
{
    public bool Succeeded { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
    public T? Data { get; set; }

    public static ApiResponse<T> Success(T data, string? message = null) => new()
    {
        Succeeded = true,
        Data = data,
        Message = message
    };

    public static ApiResponse<T> Failure(IEnumerable<string> errors, string? message = null) => new()
    {
        Succeeded = false,
        Errors = errors.ToList(),
        Message = message
    };

    public static ApiResponse<T> Failure(string error, string? message = null) =>
        Failure(new[] { error }, message);
}
