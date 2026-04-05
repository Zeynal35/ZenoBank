using Microsoft.EntityFrameworkCore;
using ZenoBank.Services.Identity.Domain.Constants;
using ZenoBank.Services.Identity.Domain.Entities;
using ZenoBank.Services.Identity.Infrastructure.Persistence;

namespace ZenoBank.Services.Identity.Infrastructure.Seeders;

public static class RoleSeeder
{
    public static async Task SeedAsync(IdentityDbContext dbContext)
    {
        var existingRoles = await dbContext.Roles.ToListAsync();

        var requiredRoles = new[]
        {
            RoleNames.SuperAdmin,
            RoleNames.Admin,
            RoleNames.Operator,
            RoleNames.Customer
        };

        foreach (var roleName in requiredRoles)
        {
            var exists = existingRoles.Any(x => x.Name == roleName);
            if (!exists)
            {
                await dbContext.Roles.AddAsync(new Role
                {
                    Name = roleName
                });
            }
        }

        await dbContext.SaveChangesAsync();
    }
}