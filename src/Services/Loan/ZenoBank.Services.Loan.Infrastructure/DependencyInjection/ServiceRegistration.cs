using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZenoBank.BuildingBlocks.Shared.Common.Abstractions;
using ZenoBank.BuildingBlocks.Shared.Messaging.DependencyInjection;
using ZenoBank.Services.Loan.Application.Abstractions.Repositories;
using ZenoBank.Services.Loan.Application.Abstractions.Services;
using ZenoBank.Services.Loan.Infrastructure.Configurations;
using ZenoBank.Services.Loan.Infrastructure.Persistence;
using ZenoBank.Services.Loan.Infrastructure.Repositories;
using ZenoBank.Services.Loan.Infrastructure.Services;

namespace ZenoBank.Services.Loan.Infrastructure.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddLoanInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServiceEndpoints>(configuration.GetSection(ServiceEndpoints.SectionName));

        services.AddDbContext<LoanDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly("ZenoBank.Services.Loan.Infrastructure");
                });
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

        services.AddScoped<ILoanRepository, LoanRepository>();
        services.AddScoped<ILoanService, LoanService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ILoanCalculator, LoanCalculator>();
        services.AddScoped<IAuditLogger, LoanAuditLogger>();

        return services;
    }
}
