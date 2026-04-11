using System.Net.Http.Json;
using System.Text.Json;
using ZenoBank.BuildingBlocks.Shared.Common.Results;
using ZenoBank.Services.Loan.Application.Abstractions.Services;
using ZenoBank.Services.Loan.Application.DTOs;

namespace ZenoBank.Services.Loan.Infrastructure.Services;

public class AccountServiceClient : IAccountServiceClient
{
    private readonly HttpClient _httpClient;

    public AccountServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<decimal>> IncreaseBalanceAsync(Guid accountId, decimal amount, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "internal/accounts/increase-balance",
                new { AccountId = accountId, Amount = amount },
                cancellationToken);

            var content = await ReadResponseAsync<object>(response, cancellationToken);

            if (!response.IsSuccessStatusCode || content is null || !content.Success)
                return Result<decimal>.Failure(
                    content?.Message ?? $"Failed to increase balance. HTTP {(int)response.StatusCode}",
                    content?.Errors);

            return Result<decimal>.Success(amount, content.Message);
        }
        catch (HttpRequestException ex)
        {
            return Result<decimal>.Failure($"Account API connection error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return Result<decimal>.Failure("Account API request timed out.");
        }
        catch (Exception ex)
        {
            return Result<decimal>.Failure($"Unexpected account service error: {ex.Message}");
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
                Message = $"Account API returned invalid JSON. HTTP {(int)response.StatusCode}. Raw: {raw}"
            };
        }
    }
}
