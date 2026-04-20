using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZenoBank.BuildingBlocks.Shared.Common.Abstractions;
using ZenoBank.BuildingBlocks.Shared.Messaging.DependencyInjection;
using ZenoBank.Services.Transaction.Application.Abstractions.Repositories;
using ZenoBank.Services.Transaction.Application.Abstractions.Services;
using ZenoBank.Services.Transaction.Infrastructure.Configurations;
using ZenoBank.Services.Transaction.Infrastructure.Persistence;
using ZenoBank.Services.Transaction.Infrastructure.Repositories;
using ZenoBank.Services.Transaction.Infrastructure.Services;

namespace ZenoBank.Services.Transaction.Infrastructure.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddTransactionInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServiceEndpoints>(configuration.GetSection(ServiceEndpoints.SectionName));

        services.AddDbContext<TransactionDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null));
        });

        services.AddHttpContextAccessor();

        services.AddHttpClient<IAccountServiceClient, AccountServiceClient>((sp, client) =>
        {
            var endpoints = configuration.GetSection(ServiceEndpoints.SectionName).Get<ServiceEndpoints>();

            if (endpoints is null || string.IsNullOrWhiteSpace(endpoints.AccountApiBaseUrl))
                throw new InvalidOperationException("ServiceEndpoints:AccountApiBaseUrl is missing in configuration.");

            client.BaseAddress = new Uri(endpoints.AccountApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(15);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });

        services.AddSharedMessaging(configuration);

        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITransactionReferenceGenerator, TransactionReferenceGenerator>();
        services.AddScoped<IAuditLogger, TransactionAuditLogger>();

        return services;
    }
}