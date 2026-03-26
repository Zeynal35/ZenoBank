using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZenoBank.Services.Account.Application.Abstractions.Repositories;
using ZenoBank.Services.Account.Application.Abstractions.Services;
using ZenoBank.Services.Account.Infrastructure.Persistence;
using ZenoBank.Services.Account.Infrastructure.Repositories;
using ZenoBank.Services.Account.Infrastructure.Services;

namespace ZenoBank.Services.Account.Infrastructure.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddAccountInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AccountDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddHttpContextAccessor();

        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<IBankAccountService, BankAccountService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAccountNumberGenerator, AccountNumberGenerator>();

        return services;
    }
}
