using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZenoBank.BuildingBlocks.Shared.Common.Abstractions;
using ZenoBank.Services.Identity.Application.Abstractions.Repositories;
using ZenoBank.Services.Identity.Application.Abstractions.Services;
using ZenoBank.Services.Identity.Infrastructure.Configurations;
using ZenoBank.Services.Identity.Infrastructure.Persistence;
using ZenoBank.Services.Identity.Infrastructure.Repositories;
using ZenoBank.Services.Identity.Infrastructure.Services;

namespace ZenoBank.Services.Identity.Infrastructure.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<SeedUserSettings>(configuration.GetSection(SeedUserSettings.SectionName));

        services.AddDbContext<IdentityDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddHttpContextAccessor();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuditLogger, IdentityAuditLogger>();

        return services;
    }
}