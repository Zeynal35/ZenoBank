using ZenoBank.BuildingBlocks.Shared.Common.Results;
using ZenoBank.Services.Loan.Application.DTOs;

namespace ZenoBank.Services.Loan.Application.Abstractions.Services;

public interface ILoanService
{
    Task<Result<LoanApplicationDto>> CreateAsync(Guid userId, CreateLoanRequest request, CancellationToken cancellationToken = default);
    Task<Result<List<LoanApplicationDto>>> GetMyLoansAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<LoanApplicationDto>> GetMyLoanByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

    Task<Result<List<LoanApplicationDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<LoanApplicationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<LoanApplicationDto>> ApproveAsync(Guid id, decimal interestRate, CancellationToken cancellationToken = default);
    Task<Result<LoanApplicationDto>> RejectAsync(Guid id, string reason, CancellationToken cancellationToken = default);
}
