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
        IdentityDbContext context,
        IOptions<SeedUserSettings> seedUserOptions)
    {
        var settings = seedUserOptions.Value;
        var passwordHasher = new PasswordHasher();

        await SeedUserIfNotExistsAsync(
            context,
            settings.SuperAdmin,
            RoleNames.SuperAdmin,
            passwordHasher);

        await SeedUserIfNotExistsAsync(
            context,
            settings.Admin,
            RoleNames.Admin,
            passwordHasher);

        await SeedUserIfNotExistsAsync(
            context,
            settings.Operator,
            RoleNames.Operator,
            passwordHasher);
    }

    private static async Task SeedUserIfNotExistsAsync(
        IdentityDbContext context,
        SeedUserItem seedUser,
        string roleName,
        PasswordHasher passwordHasher)
    {
        if (string.IsNullOrWhiteSpace(seedUser.UserName) ||
            string.IsNullOrWhiteSpace(seedUser.Email) ||
            string.IsNullOrWhiteSpace(seedUser.Password))
        {
            return;
        }

        var existingUser = await context.Users
            .Include(x => x.UserRoles)
            .FirstOrDefaultAsync(x =>
                x.UserName == seedUser.UserName ||
                x.Email == seedUser.Email);

        if (existingUser is not null)
            return;

        var role = await context.Roles.FirstOrDefaultAsync(x => x.Name == roleName);
        if (role is null)
            return;

        var user = new User
        {
            UserName = seedUser.UserName.Trim(),
            Email = seedUser.Email.Trim().ToLower(),
            PasswordHash = passwordHasher.HashPassword(seedUser.Password),
            IsActive = true
        };

        user.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            User = user,
            Role = role
        });

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }
}