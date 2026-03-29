using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZenoBank.BuildingBlocks.Shared.Common.Abstractions;
using ZenoBank.Services.Customer.Application.Abstractions.Repositories;
using ZenoBank.Services.Customer.Application.Abstractions.Services;
using ZenoBank.Services.Customer.Infrastructure.Persistence;
using ZenoBank.Services.Customer.Infrastructure.Repositories;
using ZenoBank.Services.Customer.Infrastructure.Services;

namespace ZenoBank.Services.Customer.Infrastructure.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddCustomerInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CustomerDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddHttpContextAccessor();

        services.AddScoped<ICustomerProfileRepository, CustomerProfileRepository>();
        services.AddScoped<ICustomerProfileService, CustomerProfileService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuditLogger, CustomerAuditLogger>();

        return services;
    }
}
