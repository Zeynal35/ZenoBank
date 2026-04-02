using System.Net.Http.Json;
using System.Text.Json;
using ZenoBank.BuildingBlocks.Shared.Common.Results;
using ZenoBank.Services.Loan.Application.Abstractions.Services;
using ZenoBank.Services.Loan.Application.DTOs;

namespace ZenoBank.Services.Loan.Infrastructure.Services;

public class CustomerServiceClient : ICustomerServiceClient
{
    private readonly HttpClient _httpClient;

    public CustomerServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<InternalCustomerComplianceSnapshotDto>> GetCustomerComplianceAsync(Guid customerProfileId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"internal/customers/{customerProfileId}/compliance", cancellationToken);
            var content = await ReadResponseAsync<InternalCustomerComplianceSnapshotDto>(response, cancellationToken);

            if (!response.IsSuccessStatusCode || content is null || !content.Success || content.Data is null)
                return Result<InternalCustomerComplianceSnapshotDto>.Failure(
                    content?.Message ?? $"Failed to fetch customer compliance. HTTP {(int)response.StatusCode}",
                    content?.Errors);

            return Result<InternalCustomerComplianceSnapshotDto>.Success(content.Data, content.Message);
        }
        catch (HttpRequestException ex)
        {
            return Result<InternalCustomerComplianceSnapshotDto>.Failure($"Customer API connection error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return Result<InternalCustomerComplianceSnapshotDto>.Failure("Customer API request timed out.");
        }
        catch (Exception ex)
        {
            return Result<InternalCustomerComplianceSnapshotDto>.Failure($"Unexpected customer service error: {ex.Message}");
        }
    }

    private static async Task<ServiceApiResponse<T>?> ReadResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<ServiceApiResponse<T>>(cancellationToken: cancellationToken);
        }
        catch (JsonException)
        {
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);
            return new ServiceApiResponse<T>
            {
                Success = false,
                Message = $"Customer API returned invalid JSON. HTTP {(int)response.StatusCode}. Raw: {raw}"
            };
        }
    }
}
