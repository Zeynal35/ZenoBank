using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ZenoBank.Services.Identity.Domain.Constants;
using ZenoBank.Services.Identity.Domain.Entities;
using ZenoBank.Services.Identity.Infrastructure.Configurations;
using ZenoBank.Services.Identity.Infrastructure.Persistence;
using ZenoBank.Services.Identity.Infrastructure.Services;

namespace ZenoBank.Services.Identity.Infrastructure.Seeders;

public static class UserSeeder
{
    public static async Task SeedAsync(
        IdentityDbContext dbContext,
        IOptions<SeedUserSettings> seedUserOptions)
    {
        var settings = seedUserOptions.Value;
        var passwordHasher = new PasswordHasher();

        var superAdminRole = await dbContext.Roles.FirstOrDefaultAsync(x => x.Name == RoleNames.SuperAdmin);
        var adminRole = await dbContext.Roles.FirstOrDefaultAsync(x => x.Name == RoleNames.Admin);
        var operatorRole = await dbContext.Roles.FirstOrDefaultAsync(x => x.Name == RoleNames.Operator);

        if (superAdminRole is null || adminRole is null || operatorRole is null)
            return;

        await EnsureUserAsync(
            dbContext,
            passwordHasher,
            userName: "superadmin",
            email: "superadmin@zenobank.local",
            password: "SuperAdmin123!",
            roleIds: new[] { superAdminRole.Id });

        await EnsureUserAsync(
            dbContext,
            passwordHasher,
            userName: "admin1",
            email: "admin1@zenobank.local",
            password: "Admin123!",
            roleIds: new[] { adminRole.Id });

        await EnsureUserAsync(
            dbContext,
            passwordHasher,
            userName: "admin2",
            email: "admin2@zenobank.local",
            password: "Admin123!",
            roleIds: new[] { adminRole.Id });

        await EnsureUserAsync(
            dbContext,
            passwordHasher,
            userName: "operator1",
            email: "operator1@zenobank.local",
            password: "Operator123!",
            roleIds: new[] { operatorRole.Id });

        await EnsureUserAsync(
            dbContext,
            passwordHasher,
            userName: "operator2",
            email: "operator2@zenobank.local",
            password: "Operator123!",
            roleIds: new[] { operatorRole.Id });

        await EnsureUserAsync(
            dbContext,
            passwordHasher,
            userName: "operator3",
            email: "operator3@zenobank.local",
            password: "Operator123!",
            roleIds: new[] { operatorRole.Id });

        await EnsureUserAsync(
            dbContext,
            passwordHasher,
            userName: "operator4",
            email: "operator4@zenobank.local",
            password: "Operator123!",
            roleIds: new[] { operatorRole.Id });

        await dbContext.SaveChangesAsync();
    }

    private static async Task EnsureUserAsync(
        IdentityDbContext dbContext,
        PasswordHasher passwordHasher,
        string userName,
        string email,
        string password,
        IEnumerable<Guid> roleIds)
    {
        var existingUser = await dbContext.Users
            .Include(x => x.UserRoles)
            .FirstOrDefaultAsync(x => x.UserName == userName);

        if (existingUser is null)
        {
            var user = new User
            {
                UserName = userName,
                Email = email,
                PasswordHash = passwordHasher.HashPassword(password),
                IsActive = true,
                EmailConfirmed = true
            };

            foreach (var roleId in roleIds)
            {
                user.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = roleId,
                    User = user
                });
            }

            await dbContext.Users.AddAsync(user);
            return;
        }

        existingUser.Email = email;
        existingUser.IsActive = true;
        existingUser.EmailConfirmed = true;
        existingUser.PasswordHash = passwordHasher.HashPassword(password);

        foreach (var roleId in roleIds)
        {
            var hasRole = existingUser.UserRoles.Any(x => x.RoleId == roleId);
            if (!hasRole)
            {
                existingUser.UserRoles.Add(new UserRole
                {
                    UserId = existingUser.Id,
                    RoleId = roleId
                });
            }
        }

        dbContext.Users.Update(existingUser);
    }
}