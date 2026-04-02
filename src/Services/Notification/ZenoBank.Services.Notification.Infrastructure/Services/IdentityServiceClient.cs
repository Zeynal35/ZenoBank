using System.Net.Http.Json;
using System.Text.Json;
using ZenoBank.BuildingBlocks.Shared.Common.Results;
using ZenoBank.Services.Notification.Application.Abstractions.Services;
using ZenoBank.Services.Notification.Application.DTOs;

namespace ZenoBank.Services.Notification.Infrastructure.Services;

public class IdentityServiceClient : IIdentityServiceClient
{
    private readonly HttpClient _httpClient;

    public IdentityServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<InternalUserContactSnapshotDto>> GetUserContactAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"internal/users/{userId}/contact", cancellationToken);
            var content = await ReadResponseAsync<InternalUserContactSnapshotDto>(response, cancellationToken);

            if (!response.IsSuccessStatusCode || content is null || !content.Success || content.Data is null)
                return Result<InternalUserContactSnapshotDto>.Failure(
                    content?.Message ?? $"Failed to fetch user contact. HTTP {(int)response.StatusCode}",
                    content?.Errors);

            return Result<InternalUserContactSnapshotDto>.Success(content.Data, content.Message);
        }
        catch (HttpRequestException ex)
        {
            return Result<InternalUserContactSnapshotDto>.Failure($"Identity API connection error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return Result<InternalUserContactSnapshotDto>.Failure("Identity API request timed out.");
        }
        catch (Exception ex)
        {
            return Result<InternalUserContactSnapshotDto>.Failure($"Unexpected identity service error: {ex.Message}");
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
                Message = $"Identity API returned invalid JSON. HTTP {(int)response.StatusCode}. Raw: {raw}"
            };
        }
    }
}