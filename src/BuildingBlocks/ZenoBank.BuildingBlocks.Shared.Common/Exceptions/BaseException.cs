namespace ZenoBank.BuildingBlocks.Shared.Common.Exceptions;

public abstract class BaseException : Exception
{
    protected BaseException(string message) : base(message)
    {
    }
}