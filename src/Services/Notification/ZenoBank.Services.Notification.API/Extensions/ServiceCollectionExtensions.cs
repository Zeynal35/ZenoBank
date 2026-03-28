using ZenoBank.Services.Notification.Infrastructure.DependencyInjection;

namespace ZenoBank.Services.Notification.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddNotificationInfrastructure(configuration);
        return services;
    }
}
