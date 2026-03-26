using ZenoBank.Services.Transaction.Infrastructure.DependencyInjection;

namespace ZenoBank.Services.Transaction.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTransactionModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransactionInfrastructure(configuration);
        return services;
    }
}
