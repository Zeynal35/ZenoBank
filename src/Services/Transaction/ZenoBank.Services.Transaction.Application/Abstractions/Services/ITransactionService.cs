using ZenoBank.BuildingBlocks.Shared.Common.Results;
using ZenoBank.Services.Transaction.Application.DTOs;

namespace ZenoBank.Services.Transaction.Application.Abstractions.Services;

public interface ITransactionService
{
    Task<Result<TransactionRecordDto>> DepositAsync(Guid userId, DepositRequest request, CancellationToken cancellationToken = default);
    Task<Result<TransactionRecordDto>> WithdrawAsync(Guid userId, WithdrawRequest request, CancellationToken cancellationToken = default);
    Task<Result<TransactionRecordDto>> TransferAsync(Guid userId, TransferRequest request, CancellationToken cancellationToken = default);

    Task<Result<List<TransactionRecordDto>>> GetMyTransactionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<TransactionRecordDto>> GetMyTransactionByIdAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken = default);

    Task<Result<List<TransactionRecordDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<TransactionRecordDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
