using Microsoft.EntityFrameworkCore;
using ZenoBank.Services.Identity.Domain.Constants;
using ZenoBank.Services.Identity.Domain.Entities;
using ZenoBank.Services.Identity.Infrastructure.Persistence;

namespace ZenoBank.Services.Identity.Infrastructure.Seeders;

public static class RoleSeeder
{
    public static async Task SeedAsync(IdentityDbContext context)
    {
        var existingRoleNames = await context.Roles
            .Select(x => x.Name)
            .ToListAsync();

        var rolesToSeed = new List<string>
        {
            RoleNames.SuperAdmin,
            RoleNames.Admin,
            RoleNames.Operator,
            RoleNames.Customer
        };

        var newRoles = rolesToSeed
            .Where(roleName => !existingRoleNames.Contains(roleName))
            .Select(roleName => new Role
            {
                Name = roleName
            })
            .ToList();

        if (newRoles.Count == 0)
            return;

        await context.Roles.AddRangeAsync(newRoles);
        await context.SaveChangesAsync();
    }
}