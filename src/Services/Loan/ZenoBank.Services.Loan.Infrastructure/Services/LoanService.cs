using ZenoBank.BuildingBlocks.Shared.Common.Abstractions;
using ZenoBank.BuildingBlocks.Shared.Common.DTOs;
using ZenoBank.BuildingBlocks.Shared.Common.Results;
using ZenoBank.BuildingBlocks.Shared.Contracts.Events;
using ZenoBank.BuildingBlocks.Shared.Messaging.Abstractions;
using ZenoBank.Services.Loan.Application.Abstractions.Repositories;
using ZenoBank.Services.Loan.Application.Abstractions.Services;
using ZenoBank.Services.Loan.Application.DTOs;
using ZenoBank.Services.Loan.Domain.Entities;
using ZenoBank.Services.Loan.Domain.Enums;

namespace ZenoBank.Services.Loan.Infrastructure.Services;

public class LoanService : ILoanService
{
    private readonly ILoanRepository _repository;
    private readonly ICustomerServiceClient _customerServiceClient;
    private readonly ILoanCalculator _loanCalculator;
    private readonly IAuditLogger _auditLogger;
    private readonly IEventPublisher _eventPublisher;

    public LoanService(
        ILoanRepository repository,
        ICustomerServiceClient customerServiceClient,
        ILoanCalculator loanCalculator,
        IAuditLogger auditLogger,
        IEventPublisher eventPublisher)
    {
        _repository = repository;
        _customerServiceClient = customerServiceClient;
        _loanCalculator = loanCalculator;
        _auditLogger = auditLogger;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<LoanApplicationDto>> CreateAsync(Guid userId, CreateLoanRequest request, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        if (request.CustomerProfileId == Guid.Empty)
            errors.Add("CustomerProfileId is required.");

        if (request.PrincipalAmount <= 0)
            errors.Add("PrincipalAmount must be greater than zero.");

        if (request.TermInMonths <= 0)
            errors.Add("TermInMonths must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.Currency))
            errors.Add("Currency is required.");

        if (string.IsNullOrWhiteSpace(request.Purpose))
            errors.Add("Purpose is required.");

        if (errors.Count > 0)
            return Result<LoanApplicationDto>.Failure("Validation failed.", errors);

        var complianceResult = await _customerServiceClient.GetCustomerComplianceAsync(request.CustomerProfileId, cancellationToken);
        if (complianceResult.IsFailure || complianceResult.Data is null)
            return Result<LoanApplicationDto>.Failure(complianceResult.Message, complianceResult.Errors);

        var compliance = complianceResult.Data;

        if (compliance.UserId != userId)
            return Result<LoanApplicationDto>.Failure("Customer profile does not belong to the current user.");

        if (!compliance.IsEligibleForBanking)
        {
            if (compliance.Age < 18)
                return Result<LoanApplicationDto>.Failure("Customer must be at least 18 years old to request a loan.");

            if (!compliance.IsKycApproved)
                return Result<LoanApplicationDto>.Failure($"KYC is not approved. Current KYC status: {compliance.KycStatus}");

            if (compliance.IsBlacklisted)
                return Result<LoanApplicationDto>.Failure($"Customer is blacklisted. Reason: {compliance.BlacklistReason}");

            return Result<LoanApplicationDto>.Failure("Customer is not eligible for loan operations.");
        }

        if (string.Equals(compliance.RiskLevel, "High", StringComparison.OrdinalIgnoreCase))
            return Result<LoanApplicationDto>.Failure("High risk customers cannot request a loan.");

        var loan = new LoanApplication
        {
            UserId = userId,
            CustomerProfileId = request.CustomerProfileId,
            PrincipalAmount = request.PrincipalAmount,
            InterestRate = 0m,
            TermInMonths = request.TermInMonths,
            MonthlyPayment = 0m,
            TotalRepayment = 0m,
            Currency = request.Currency.Trim().ToUpper(),
            Purpose = request.Purpose.Trim(),
            Status = LoanStatus.Pending
        };

        await _repository.AddAsync(loan, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        await _auditLogger.WriteAsync(new CreateAuditLogRequest
        {
            UserId = userId,
            Action = "LoanApplicationCreated",
            EntityType = "LoanApplication",
            EntityId = loan.Id.ToString(),
            Description = $"Loan application created. Principal: {loan.PrincipalAmount} {loan.Currency}, Term: {loan.TermInMonths} months.",
            Status = "Success"
        }, cancellationToken);

        return Result<LoanApplicationDto>.Success(Map(loan), "Loan application created successfully.");
    }

    public async Task<Result<List<LoanApplicationDto>>> GetMyLoansAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var loans = await _repository.GetByUserIdAsync(userId, cancellationToken);
        return Result<List<LoanApplicationDto>>.Success(loans.Select(Map).ToList(), "Loans fetched successfully.");
    }

    public async Task<Result<LoanApplicationDto>> GetMyLoanByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var loan = await _repository.GetByIdAsync(id, cancellationToken);
        if (loan is null)
            return Result<LoanApplicationDto>.Failure("Loan application not found.");

        if (loan.UserId != userId)
            return Result<LoanApplicationDto>.Failure("You are not allowed to access this loan.");

        return Result<LoanApplicationDto>.Success(Map(loan), "Loan fetched successfully.");
    }

    public async Task<Result<List<LoanApplicationDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var loans = await _repository.GetAllAsync(cancellationToken);
        return Result<List<LoanApplicationDto>>.Success(loans.Select(Map).ToList(), "Loans fetched successfully.");
    }

    public async Task<Result<LoanApplicationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var loan = await _repository.GetByIdAsync(id, cancellationToken);
        if (loan is null)
            return Result<LoanApplicationDto>.Failure("Loan application not found.");

        return Result<LoanApplicationDto>.Success(Map(loan), "Loan fetched successfully.");
    }

    public async Task<Result<LoanApplicationDto>> ApproveAsync(Guid id, decimal interestRate, CancellationToken cancellationToken = default)
    {
        if (interestRate < 0)
            return Result<LoanApplicationDto>.Failure("InterestRate cannot be negative.");

        var loan = await _repository.GetByIdAsync(id, cancellationToken);
        if (loan is null)
            return Result<LoanApplicationDto>.Failure("Loan application not found.");

        if (loan.Status != LoanStatus.Pending)
            return Result<LoanApplicationDto>.Failure("Only pending loan applications can be approved.");

        loan.InterestRate = interestRate;
        loan.MonthlyPayment = _loanCalculator.CalculateMonthlyPayment(loan.PrincipalAmount, interestRate, loan.TermInMonths);
        loan.TotalRepayment = _loanCalculator.CalculateTotalRepayment(loan.MonthlyPayment, loan.TermInMonths);
        loan.Status = LoanStatus.Approved;
        loan.ApprovedAtUtc = DateTime.UtcNow;
        loan.UpdatedAtUtc = DateTime.UtcNow;

        _repository.Update(loan);
        await _repository.SaveChangesAsync(cancellationToken);

        await _auditLogger.WriteAsync(new CreateAuditLogRequest
        {
            UserId = loan.UserId,
            Action = "LoanApplicationApproved",
            EntityType = "LoanApplication",
            EntityId = loan.Id.ToString(),
            Description = $"Loan approved with {loan.InterestRate}% interest. Monthly payment: {loan.MonthlyPayment} {loan.Currency}.",
            Status = "Success"
        }, cancellationToken);

        await _eventPublisher.PublishAsync(new LoanApprovedEvent
        {
            UserId = loan.UserId,
            LoanId = loan.Id,
            PrincipalAmount = loan.PrincipalAmount,
            InterestRate = loan.InterestRate,
            MonthlyPayment = loan.MonthlyPayment,
            TotalRepayment = loan.TotalRepayment,
            TermInMonths = loan.TermInMonths,
            Currency = loan.Currency,
            ApprovedAtUtc = loan.ApprovedAtUtc!.Value
        }, cancellationToken);

        return Result<LoanApplicationDto>.Success(Map(loan), "Loan application approved successfully.");
    }

    public async Task<Result<LoanApplicationDto>> RejectAsync(Guid id, string reason, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return Result<LoanApplicationDto>.Failure("Rejection reason is required.");

        var loan = await _repository.GetByIdAsync(id, cancellationToken);
        if (loan is null)
            return Result<LoanApplicationDto>.Failure("Loan application not found.");

        if (loan.Status != LoanStatus.Pending)
            return Result<LoanApplicationDto>.Failure("Only pending loan applications can be rejected.");

        loan.Status = LoanStatus.Rejected;
        loan.RejectionReason = reason.Trim();
        loan.RejectedAtUtc = DateTime.UtcNow;
        loan.UpdatedAtUtc = DateTime.UtcNow;

        _repository.Update(loan);
        await _repository.SaveChangesAsync(cancellationToken);

        await _auditLogger.WriteAsync(new CreateAuditLogRequest
        {
            UserId = loan.UserId,
            Action = "LoanApplicationRejected",
            EntityType = "LoanApplication",
            EntityId = loan.Id.ToString(),
            Description = $"Loan rejected. Reason: {loan.RejectionReason}",
            Status = "Success"
        }, cancellationToken);

        await _eventPublisher.PublishAsync(new LoanRejectedEvent
        {
            UserId = loan.UserId,
            LoanId = loan.Id,
            PrincipalAmount = loan.PrincipalAmount,
            TermInMonths = loan.TermInMonths,
            Currency = loan.Currency,
            Reason = loan.RejectionReason!,
            RejectedAtUtc = loan.RejectedAtUtc!.Value
        }, cancellationToken);

        return Result<LoanApplicationDto>.Success(Map(loan), "Loan application rejected successfully.");
    }

    private static LoanApplicationDto Map(LoanApplication loan)
    {
        return new LoanApplicationDto
        {
            Id = loan.Id,
            UserId = loan.UserId,
            CustomerProfileId = loan.CustomerProfileId,
            PrincipalAmount = loan.PrincipalAmount,
            InterestRate = loan.InterestRate,
            TermInMonths = loan.TermInMonths,
            MonthlyPayment = loan.MonthlyPayment,
            TotalRepayment = loan.TotalRepayment,
            Currency = loan.Currency,
            Purpose = loan.Purpose,
            Status = loan.Status.ToString(),
            RejectionReason = loan.RejectionReason,
            CreatedAtUtc = loan.CreatedAtUtc,
            ApprovedAtUtc = loan.ApprovedAtUtc,
            RejectedAtUtc = loan.RejectedAtUtc
        };
    }
}