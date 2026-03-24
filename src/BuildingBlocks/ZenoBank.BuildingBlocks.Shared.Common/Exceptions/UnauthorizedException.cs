namespace ZenoBank.BuildingBlocks.Shared.Common.Exceptions;

public class UnauthorizedException : BaseException
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}
