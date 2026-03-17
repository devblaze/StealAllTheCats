namespace StealAllTheCats.Models.Results;

public class Result<T>
{
    public T? Data { get; private set; }
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }

    private Result() { }

    public static Result<T> Success(T data) => new() { Data = data, IsSuccess = true };
    public static Result<T> Failure(string errorMessage) => new() { ErrorMessage = errorMessage, IsSuccess = false };
}