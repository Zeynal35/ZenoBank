namespace ZenoBank.BuildingBlocks.Shared.Common.Exceptions;

public class ValidationException : BaseException
{
    public List<string> Errors { get; }

    public ValidationException(string message, List<string>? errors = null) : base(message)
    {
        Errors = errors ?? new List<string>();
    }
}
