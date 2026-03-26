using ZenoBank.Services.Account.Infrastructure.DependencyInjection;

namespace ZenoBank.Services.Account.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccountModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAccountInfrastructure(configuration);
        return services;
    }
}
