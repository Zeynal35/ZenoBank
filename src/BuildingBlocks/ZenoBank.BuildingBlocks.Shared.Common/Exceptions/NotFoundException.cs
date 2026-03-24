namespace ZenoBank.BuildingBlocks.Shared.Common.Exceptions;

public class NotFoundException : BaseException
{
    public NotFoundException(string message) : base(message)
    {
    }
}