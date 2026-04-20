using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZenoBank.BuildingBlocks.Shared.Common.Abstractions;
using ZenoBank.BuildingBlocks.Shared.Messaging.DependencyInjection;
using ZenoBank.Services.Account.Application.Abstractions.Repositories;
using ZenoBank.Services.Account.Application.Abstractions.Services;
using ZenoBank.Services.Account.Infrastructure.Configurations;
using ZenoBank.Services.Account.Infrastructure.Persistence;
using ZenoBank.Services.Account.Infrastructure.Repositories;
using ZenoBank.Services.Account.Infrastructure.Services;

namespace ZenoBank.Services.Account.Infrastructure.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddAccountInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServiceEndpoints>(configuration.GetSection(ServiceEndpoints.SectionName));

        services.AddDbContext<AccountDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null));
        });

        services.AddHttpContextAccessor();
        services.AddSharedMessaging(configuration);

        services.AddHttpClient<ICustomerServiceClient, CustomerServiceClient>((sp, client) =>
        {
            var endpoints = configuration.GetSection(ServiceEndpoints.SectionName).Get<ServiceEndpoints>();

            if (endpoints is null || string.IsNullOrWhiteSpace(endpoints.CustomerApiBaseUrl))
                throw new InvalidOperationException("ServiceEndpoints:CustomerApiBaseUrl is missing in configuration.");

            client.BaseAddress = new Uri(endpoints.CustomerApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(15);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });

        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<IBankAccountService, BankAccountService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAccountNumberGenerator, AccountNumberGenerator>();
        services.AddScoped<IAuditLogger, AccountAuditLogger>();

        return services;
    }
}