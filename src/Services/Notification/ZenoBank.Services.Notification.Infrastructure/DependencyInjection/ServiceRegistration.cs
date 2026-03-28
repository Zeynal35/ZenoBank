using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZenoBank.BuildingBlocks.Shared.Messaging.Configurations;
using ZenoBank.Services.Notification.Application.Abstractions.Repositories;
using ZenoBank.Services.Notification.Application.Abstractions.Services;
using ZenoBank.Services.Notification.Infrastructure.BackgroundWorkers;
using ZenoBank.Services.Notification.Infrastructure.Persistence;
using ZenoBank.Services.Notification.Infrastructure.Repositories;
using ZenoBank.Services.Notification.Infrastructure.Services;

namespace ZenoBank.Services.Notification.Infrastructure.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));

        services.AddDbContext<NotificationDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddHttpContextAccessor();

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddHostedService<TransactionEventsConsumer>();

        return services;
    }
}
