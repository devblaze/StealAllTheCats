namespace StealAllTheCats.Dtos;

public class Result<T>
{
    public bool Success { get; }
    public T? Data { get; }
    public string? ErrorMessage { get; }
    public int ErrorCode { get; }
    public Exception? Exception { get; }

    protected Result(bool success, T? data, string? errorMessage = null, int errorCode = 200, Exception? exception = null)
    {
        Success = success;
        Data = data;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        Exception = exception;
    }

    public static Result<T> Ok(T data) => new(true, data);

    public static Result<T> Fail(string message, int errorCode = 500, Exception? exception = null) =>
        new(false, default, message, errorCode, exception);
}