namespace ZenoBank.Services.Identity.Application.DTOs;

public class AssignRoleRequest
{
    public Guid UserId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}
