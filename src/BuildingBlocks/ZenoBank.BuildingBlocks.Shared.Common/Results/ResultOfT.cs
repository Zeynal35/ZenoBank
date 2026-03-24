namespace ZenoBank.BuildingBlocks.Shared.Common.Results;

public class Result<T> : Result
{
    public T? Data { get; private set; }

    public static Result<T> Success(T data, string message = "Operation completed successfully.")
    {
        return new Result<T>
        {
            IsSuccess = true,
            Message = message,
            Data = data
        };
    }

    public new static Result<T> Failure(string message, List<string>? errors = null)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}