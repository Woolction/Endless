namespace Application;

public class Result<T>
{
    public T? Data { get; }
    public string? Error { get; }
    public int StatusCode { get; }

    public bool IsSuccess => Error == null;

    public Result(T data, int statusCode)
    {
        Data = data;
        StatusCode = statusCode;
    }

    public Result(int statusCode, string error)
    {
        StatusCode = statusCode;
        Error = error;
    }

    public Result()
    {
    }

    public static Result<T> Success(int statusCode, T data) =>
        new(data, statusCode);

    public static Result<T> Failure(int statusCode, string error) =>
        new(statusCode, error);
}

public class Null { }