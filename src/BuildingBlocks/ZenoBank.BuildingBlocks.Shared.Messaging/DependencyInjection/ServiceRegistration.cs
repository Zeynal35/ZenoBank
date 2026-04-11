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
        services.Configure<RabbitMqSettings>(options =>
        {
            configuration.GetSection(RabbitMqSettings.SectionName).Bind(options);
        });

        services.AddSingleton<RabbitMqConnection>();
        services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();

        return services;
    }
}