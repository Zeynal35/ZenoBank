using System.Net.Http.Json;
using System.Text.Json;
using ZenoBank.BuildingBlocks.Shared.Common.Results;
using ZenoBank.Services.Transaction.Application.Abstractions.Services;
using ZenoBank.Services.Transaction.Application.DTOs;

namespace ZenoBank.Services.Transaction.Infrastructure.Services;

public class AccountServiceClient : IAccountServiceClient
{
    private readonly HttpClient _httpClient;

    public AccountServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<InternalAccountSnapshotDto>> GetAccountByIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"internal/accounts/{accountId}", cancellationToken);
            var content = await ReadResponseAsync<InternalAccountSnapshotDto>(response, cancellationToken);

            if (!response.IsSuccessStatusCode || content is null || !content.Success || content.Data is null)
                return Result<InternalAccountSnapshotDto>.Failure(
                    content?.Message ?? $"Failed to fetch account. HTTP {(int)response.StatusCode}",
                    content?.Errors);

            return Result<InternalAccountSnapshotDto>.Success(content.Data, content.Message);
        }
        catch (HttpRequestException ex)
        {
            return Result<InternalAccountSnapshotDto>.Failure($"Account API connection error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return Result<InternalAccountSnapshotDto>.Failure("Account API request timed out.");
        }
        catch (Exception ex)
        {
            return Result<InternalAccountSnapshotDto>.Failure($"Unexpected account service error: {ex.Message}");
        }
    }

    public async Task<Result<AccountBalanceSnapshotDto>> IncreaseBalanceAsync(Guid accountId, decimal amount, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "internal/accounts/increase-balance",
                new { AccountId = accountId, Amount = amount },
                cancellationToken);

            var content = await ReadResponseAsync<AccountBalanceSnapshotDto>(response, cancellationToken);

            if (!response.IsSuccessStatusCode || content is null || !content.Success || content.Data is null)
                return Result<AccountBalanceSnapshotDto>.Failure(
                    content?.Message ?? $"Failed to increase balance. HTTP {(int)response.StatusCode}",
                    content?.Errors);

            return Result<AccountBalanceSnapshotDto>.Success(content.Data, content.Message);
        }
        catch (HttpRequestException ex)
        {
            return Result<AccountBalanceSnapshotDto>.Failure($"Account API connection error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return Result<AccountBalanceSnapshotDto>.Failure("Account API request timed out.");
        }
        catch (Exception ex)
        {
            return Result<AccountBalanceSnapshotDto>.Failure($"Unexpected account service error: {ex.Message}");
        }
    }

    public async Task<Result<AccountBalanceSnapshotDto>> DecreaseBalanceAsync(Guid accountId, decimal amount, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "internal/accounts/decrease-balance",
                new { AccountId = accountId, Amount = amount },
                cancellationToken);

            var content = await ReadResponseAsync<AccountBalanceSnapshotDto>(response, cancellationToken);

            if (!response.IsSuccessStatusCode || content is null || !content.Success || content.Data is null)
                return Result<AccountBalanceSnapshotDto>.Failure(
                    content?.Message ?? $"Failed to decrease balance. HTTP {(int)response.StatusCode}",
                    content?.Errors);

            return Result<AccountBalanceSnapshotDto>.Success(content.Data, content.Message);
        }
        catch (HttpRequestException ex)
        {
            return Result<AccountBalanceSnapshotDto>.Failure($"Account API connection error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return Result<AccountBalanceSnapshotDto>.Failure("Account API request timed out.");
        }
        catch (Exception ex)
        {
            return Result<AccountBalanceSnapshotDto>.Failure($"Unexpected account service error: {ex.Message}");
        }
    }

    public async Task<Result> TransferBalanceAsync(Guid fromAccountId, Guid toAccountId, decimal amount, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "internal/accounts/transfer-balance",
                new { FromAccountId = fromAccountId, ToAccountId = toAccountId, Amount = amount },
                cancellationToken);

            var content = await ReadResponseAsync<object>(response, cancellationToken);

            if (!response.IsSuccessStatusCode || content is null || !content.Success)
                return Result.Failure(
                    content?.Message ?? $"Failed to transfer balance. HTTP {(int)response.StatusCode}",
                    content?.Errors);

            return Result.Success(content.Message);
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure($"Account API connection error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return Result.Failure("Account API request timed out.");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Unexpected account service error: {ex.Message}");
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
