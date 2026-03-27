using System.Net.Http.Json;
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

            var content = await response.Content.ReadFromJsonAsync<ServiceApiResponse<InternalAccountSnapshotDto>>(
                cancellationToken: cancellationToken);

            if (!response.IsSuccessStatusCode || content is null || !content.Success || content.Data is null)
                return Result<InternalAccountSnapshotDto>.Failure(
                    content?.Message ?? "Failed to fetch account.",
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

            var content = await response.Content.ReadFromJsonAsync<ServiceApiResponse<AccountBalanceSnapshotDto>>(
                cancellationToken: cancellationToken);

            if (!response.IsSuccessStatusCode || content is null || !content.Success || content.Data is null)
                return Result<AccountBalanceSnapshotDto>.Failure(
                    content?.Message ?? "Failed to increase balance.",
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

            var content = await response.Content.ReadFromJsonAsync<ServiceApiResponse<AccountBalanceSnapshotDto>>(
                cancellationToken: cancellationToken);

            if (!response.IsSuccessStatusCode || content is null || !content.Success || content.Data is null)
                return Result<AccountBalanceSnapshotDto>.Failure(
                    content?.Message ?? "Failed to decrease balance.",
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

            var content = await response.Content.ReadFromJsonAsync<ServiceApiResponse<object>>(
                cancellationToken: cancellationToken);

            if (!response.IsSuccessStatusCode || content is null || !content.Success)
                return Result.Failure(
                    content?.Message ?? "Failed to transfer balance.",
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
}
