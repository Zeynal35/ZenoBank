using ZenoBank.Services.Identity.Infrastructure.DependencyInjection;

namespace ZenoBank.Services.Identity.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityInfrastructure(configuration);
        return services;
    }
}}