using ZenoBank.Services.Loan.Infrastructure.DependencyInjection;

namespace ZenoBank.Services.Loan.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLoanModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLoanInfrastructure(configuration);
        return services;
    }
}
