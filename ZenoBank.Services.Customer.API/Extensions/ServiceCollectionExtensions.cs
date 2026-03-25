using ZenoBank.Services.Customer.Infrastructure.DependencyInjection;

namespace ZenoBank.Services.Customer.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCustomerInfrastructure(configuration);
        return services;
    }
}
