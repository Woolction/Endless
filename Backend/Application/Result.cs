namespace Application;

public class Result<T>
{
    public T? Data { get; }
    public string? Error { get; }
    public int StatusCode { get; }
    public int ResultId { get; }

    public bool IsSuccess => Error == null;

    public Result(T data, int statusCode, int resultId = 0)
    {
        Data = data;
        StatusCode = statusCode;
        ResultId = resultId;
    }

    public Result(int statusCode, string error, int resultId = 0)
    {
        StatusCode = statusCode;
        Error = error;
        ResultId = resultId;
    }

    public static Result<T> Success(int statusCode, T data, int resultId = 0) =>
        new(data, statusCode, resultId);

    public static Result<T> Failure(int statusCode, string error, int resultId = 0) =>
        new(statusCode, error, resultId);
}