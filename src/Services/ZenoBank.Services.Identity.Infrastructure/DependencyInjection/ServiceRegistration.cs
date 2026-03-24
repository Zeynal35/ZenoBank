using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZenoBank.Services.Identity.Application.Abstractions.Repositories;
using ZenoBank.Services.Identity.Application.Abstractions.Services;
using ZenoBank.Services.Identity.Infrastructure.Persistence;
using ZenoBank.Services.Identity.Infrastructure.Repositories;
using ZenoBank.Services.Identity.Infrastructure.Services;

namespace ZenoBank.Services.Identity.Infrastructure.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IdentityDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddHttpContextAccessor();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}