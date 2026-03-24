namespace ZenoBank.BuildingBlocks.Shared.Common.Results;

public class Result
{
    public bool IsSuccess { get; protected set; }
    public bool IsFailure => !IsSuccess;
    public string Message { get; protected set; } = string.Empty;
    public List<string> Errors { get; protected set; } = new();

    public static Result Success(string message = "Operation completed successfully.")
    {
        return new Result
        {
            IsSuccess = true,
            Message = message
        };
    }

    public static Result Failure(string message, List<string>? errors = null)
    {
        return new Result
        {
            IsSuccess = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}