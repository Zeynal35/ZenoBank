namespace ZenoBank.Services.Account.Application.Abstractions.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    List<string> Roles { get; }
}