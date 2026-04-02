using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZenoBank.BuildingBlocks.Shared.Messaging.Configurations;
using ZenoBank.Services.Notification.Application.Abstractions.Repositories;
using ZenoBank.Services.Notification.Application.Abstractions.Services;
using ZenoBank.Services.Notification.Infrastructure.BackgroundWorkers;
using ZenoBank.Services.Notification.Infrastructure.Configurations;
using ZenoBank.Services.Notification.Infrastructure.Persistence;
using ZenoBank.Services.Notification.Infrastructure.Repositories;
using ZenoBank.Services.Notification.Infrastructure.Services;

namespace ZenoBank.Services.Notification.Infrastructure.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));
        services.Configure<ServiceEndpoints>(configuration.GetSection(ServiceEndpoints.SectionName));
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));

        services.AddDbContext<NotificationDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddHttpContextAccessor();

        services.AddHttpClient<IIdentityServiceClient, IdentityServiceClient>((sp, client) =>
        {
            var endpoints = configuration.GetSection(ServiceEndpoints.SectionName).Get<ServiceEndpoints>();

            if (endpoints is null || string.IsNullOrWhiteSpace(endpoints.IdentityApiBaseUrl))
                throw new InvalidOperationException("ServiceEndpoints:IdentityApiBaseUrl is missing in configuration.");

            client.BaseAddress = new Uri(endpoints.IdentityApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(15);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        services.AddHostedService<TransactionEventsConsumer>();

        return services;
    }
}
