using ZenoBank.BuildingBlocks.Shared.Common.Entities;

namespace ZenoBank.Services.Identity.Domain.Entities;

public class Role : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}