using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZenoBank.BuildingBlocks.Shared.Messaging.Abstractions;
using ZenoBank.BuildingBlocks.Shared.Messaging.Configurations;
using ZenoBank.BuildingBlocks.Shared.Messaging.Services;

namespace ZenoBank.BuildingBlocks.Shared.Messaging.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddSharedMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));
        services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();

        return services;
    }
}
