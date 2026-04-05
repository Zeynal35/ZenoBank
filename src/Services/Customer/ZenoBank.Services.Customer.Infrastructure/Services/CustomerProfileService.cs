using ZenoBank.BuildingBlocks.Shared.Common.Abstractions;
using ZenoBank.BuildingBlocks.Shared.Common.DTOs;
using ZenoBank.BuildingBlocks.Shared.Common.Results;
using ZenoBank.Services.Customer.Application.Abstractions.Repositories;
using ZenoBank.Services.Customer.Application.Abstractions.Services;
using ZenoBank.Services.Customer.Application.DTOs;
using ZenoBank.Services.Customer.Domain.Entities;
using ZenoBank.Services.Customer.Domain.Enums;

namespace ZenoBank.Services.Customer.Infrastructure.Services;

public class CustomerProfileService : ICustomerProfileService
{
    private readonly ICustomerProfileRepository _repository;
    private readonly IKycDocumentRepository _kycDocumentRepository;
    private readonly IAuditLogger _auditLogger;

    public CustomerProfileService(
        ICustomerProfileRepository repository,
        IKycDocumentRepository kycDocumentRepository,
        IAuditLogger auditLogger)
    {
        _repository = repository;
        _kycDocumentRepository = kycDocumentRepository;
        _auditLogger = auditLogger;
    }

    public async Task<Result<CustomerProfileDto>> CreateAsync(Guid userId, CreateCustomerProfileRequest request, CancellationToken cancellationToken = default)
    {
        var errors = ValidateCreateOrUpdate(
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.Address,
            request.DateOfBirth);

        if (errors.Count > 0)
            return Result<CustomerProfileDto>.Failure("Validation failed.", errors);

        var age = CalculateAge(request.DateOfBirth);
        if (age < 18)
            return Result<CustomerProfileDto>.Failure("Customer must be at least 18 years old.");

        var existingProfile = await _repository.GetByUserIdAsync(userId, cancellationToken);
        if (existingProfile is not null)
            return Result<CustomerProfileDto>.Failure("Customer profile already exists for this user.");

        var profile = new CustomerProfile
        {
            UserId = userId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            DateOfBirth = request.DateOfBirth,
            PhoneNumber = request.PhoneNumber.Trim(),
            Address = request.Address.Trim(),
            Status = CustomerStatus.Active,
            IsBlacklisted = false,
            BlacklistReason = null,
            RiskLevel = RiskLevel.Low
        };

        await _repository.AddAsync(profile, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        await _auditLogger.WriteAsync(new CreateAuditLogRequest
        {
            UserId = userId,
            Action = "CustomerProfileCreated",
            EntityType = "CustomerProfile",
            EntityId = profile.Id.ToString(),
            Description = $"Customer profile created for user {userId}.",
            Status = "Success"
        }, cancellationToken);

        return Result<CustomerProfileDto>.Success(Map(profile), "Customer profile created successfully.");
    }

    public async Task<Result<CustomerProfileDto>> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await _repository.GetByUserIdAsync(userId, cancellationToken);
        if (profile is null)
            return Result<CustomerProfileDto>.Failure("Customer profile not found.");

        return Result<CustomerProfileDto>.Success(Map(profile), "Customer profile fetched successfully.");
    }

    public async Task<Result<CustomerProfileDto>> UpdateMyProfileAsync(Guid userId, UpdateCustomerProfileRequest request, CancellationToken cancellationToken = default)
    {
        var errors = ValidateCreateOrUpdate(
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.Address,
            request.DateOfBirth);

        if (errors.Count > 0)
            return Result<CustomerProfileDto>.Failure("Validation failed.", errors);

        var age = CalculateAge(request.DateOfBirth);
        if (age < 18)
            return Result<CustomerProfileDto>.Failure("Customer must be at least 18 years old.");

        var profile = await _repository.GetByUserIdAsync(userId, cancellationToken);
        if (profile is null)
            return Result<CustomerProfileDto>.Failure("Customer profile not found.");

        profile.FirstName = request.FirstName.Trim();
        profile.LastName = request.LastName.Trim();
        profile.DateOfBirth = request.DateOfBirth;
        profile.PhoneNumber = request.PhoneNumber.Trim();
        profile.Address = request.Address.Trim();
        profile.UpdatedAtUtc = DateTime.UtcNow;

        _repository.Update(profile);
        await _repository.SaveChangesAsync(cancellationToken);

        await _auditLogger.WriteAsync(new CreateAuditLogRequest
        {
            UserId = userId,
            Action = "CustomerProfileUpdated",
            EntityType = "CustomerProfile",
            EntityId = profile.Id.ToString(),
            Description = $"Customer profile updated for user {userId}.",
            Status = "Success"
        }, cancellationToken);

        return Result<CustomerProfileDto>.Success(Map(profile), "Customer profile updated successfully.");
    }

    public async Task<Result<List<CustomerProfileDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var profiles = await _repository.GetAllAsync(cancellationToken);
        return Result<List<CustomerProfileDto>>.Success(profiles.Select(Map).ToList(), "Customer profiles fetched successfully.");
    }

    public async Task<Result<CustomerProfileDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var profile = await _repository.GetByIdAsync(id, cancellationToken);
        if (profile is null)
            return Result<CustomerProfileDto>.Failure("Customer profile not found.");

        return Result<CustomerProfileDto>.Success(Map(profile), "Customer profile fetched successfully.");
    }

    public async Task<Result<InternalCustomerComplianceDto>> GetInternalComplianceByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var profile = await _repository.GetByIdAsync(id, cancellationToken);
        if (profile is null)
            return Result<InternalCustomerComplianceDto>.Failure("Customer profile not found.");

        var latestKyc = await _kycDocumentRepository.GetLatestByCustomerProfileIdAsync(id, cancellationToken);
        var kycApproved = latestKyc?.Status == KycDocumentStatus.Approved;
        var kycStatus = latestKyc?.Status.ToString() ?? "NotSubmitted";

        var age = CalculateAge(profile.DateOfBirth);
        var isEligible =
            age >= 18 &&
            !profile.IsBlacklisted &&
            profile.Status == CustomerStatus.Active &&
            kycApproved;

        var dto = new InternalCustomerComplianceDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            DateOfBirth = profile.DateOfBirth,
            Age = age,
            Status = profile.Status.ToString(),
            IsBlacklisted = profile.IsBlacklisted,
            BlacklistReason = profile.BlacklistReason,
            RiskLevel = profile.RiskLevel.ToString(),
            IsKycApproved = kycApproved,
            KycStatus = kycStatus,
            IsEligibleForBanking = isEligible
        };

        return Result<InternalCustomerComplianceDto>.Success(dto, "Customer compliance fetched successfully.");
    }

    public async Task<Result<CustomerProfileDto>> BlacklistAsync(Guid id, string reason, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return Result<CustomerProfileDto>.Failure("Blacklist reason is required.");

        var profile = await _repository.GetByIdAsync(id, cancellationToken);
        if (profile is null)
            return Result<CustomerProfileDto>.Failure("Customer profile not found.");

        if (profile.IsBlacklisted)
            return Result<CustomerProfileDto>.Failure("Customer is already blacklisted.");

        profile.IsBlacklisted = true;
        profile.BlacklistReason = reason.Trim();
        profile.UpdatedAtUtc = DateTime.UtcNow;

        _repository.Update(profile);
        await _repository.SaveChangesAsync(cancellationToken);

        await _auditLogger.WriteAsync(new CreateAuditLogRequest
        {
            UserId = profile.UserId,
            Action = "CustomerBlacklisted",
            EntityType = "CustomerProfile",
            EntityId = profile.Id.ToString(),
            Description = $"Customer blacklisted. Reason: {profile.BlacklistReason}",
            Status = "Success"
        }, cancellationToken);

        return Result<CustomerProfileDto>.Success(Map(profile), "Customer blacklisted successfully.");
    }

    public async Task<Result<CustomerProfileDto>> RemoveFromBlacklistAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var profile = await _repository.GetByIdAsync(id, cancellationToken);
        if (profile is null)
            return Result<CustomerProfileDto>.Failure("Customer profile not found.");

        if (!profile.IsBlacklisted)
            return Result<CustomerProfileDto>.Failure("Customer is not blacklisted.");

        profile.IsBlacklisted = false;
        profile.BlacklistReason = null;
        profile.UpdatedAtUtc = DateTime.UtcNow;

        _repository.Update(profile);
        await _repository.SaveChangesAsync(cancellationToken);

        await _auditLogger.WriteAsync(new CreateAuditLogRequest
        {
            UserId = profile.UserId,
            Action = "CustomerUnblacklisted",
            EntityType = "CustomerProfile",
            EntityId = profile.Id.ToString(),
            Description = "Customer removed from blacklist.",
            Status = "Success"
        }, cancellationToken);

        return Result<CustomerProfileDto>.Success(Map(profile), "Customer removed from blacklist successfully.");
    }

    public async Task<Result<CustomerProfileDto>> UpdateRiskLevelAsync(Guid id, int riskLevel, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(typeof(RiskLevel), riskLevel))
            return Result<CustomerProfileDto>.Failure("Invalid risk level.");

        var profile = await _repository.GetByIdAsync(id, cancellationToken);
        if (profile is null)
            return Result<CustomerProfileDto>.Failure("Customer profile not found.");

        profile.RiskLevel = (RiskLevel)riskLevel;
        profile.UpdatedAtUtc = DateTime.UtcNow;

        _repository.Update(profile);
        await _repository.SaveChangesAsync(cancellationToken);

        await _auditLogger.WriteAsync(new CreateAuditLogRequest
        {
            UserId = profile.UserId,
            Action = "CustomerRiskUpdated",
            EntityType = "CustomerProfile",
            EntityId = profile.Id.ToString(),
            Description = $"Customer risk level updated to {profile.RiskLevel}.",
            Status = "Success"
        }, cancellationToken);

        return Result<CustomerProfileDto>.Success(Map(profile), "Customer risk level updated successfully.");
    }

    public async Task<Result<CustomerProfileDto>> UpdateStatusAsync(Guid id, int status, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(typeof(CustomerStatus), status))
            return Result<CustomerProfileDto>.Failure("Invalid customer status.");

        var profile = await _repository.GetByIdAsync(id, cancellationToken);
        if (profile is null)
            return Result<CustomerProfileDto>.Failure("Customer profile not found.");

        profile.Status = (CustomerStatus)status;
        profile.UpdatedAtUtc = DateTime.UtcNow;

        _repository.Update(profile);
        await _repository.SaveChangesAsync(cancellationToken);

        await _auditLogger.WriteAsync(new CreateAuditLogRequest
        {
            UserId = profile.UserId,
            Action = "CustomerStatusUpdated",
            EntityType = "CustomerProfile",
            EntityId = profile.Id.ToString(),
            Description = $"Customer status updated to {profile.Status}.",
            Status = "Success"
        }, cancellationToken);

        return Result<CustomerProfileDto>.Success(Map(profile), "Customer status updated successfully.");
    }

    private static CustomerProfileDto Map(CustomerProfile profile)
    {
        return new CustomerProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            DateOfBirth = profile.DateOfBirth,
            PhoneNumber = profile.PhoneNumber,
            Address = profile.Address,
            Status = profile.Status.ToString(),
            IsBlacklisted = profile.IsBlacklisted,
            BlacklistReason = profile.BlacklistReason,
            RiskLevel = profile.RiskLevel.ToString(),
            Age = CalculateAge(profile.DateOfBirth)
        };
    }

    private static int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.UtcNow.Date;
        var age = today.Year - dateOfBirth.Year;

        if (dateOfBirth.Date > today.AddYears(-age))
            age--;

        return age;
    }

    private static List<string> ValidateCreateOrUpdate(
        string firstName,
        string lastName,
        string phoneNumber,
        string address,
        DateTime dateOfBirth)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(firstName))
            errors.Add("First name is required.");

        if (string.IsNullOrWhiteSpace(lastName))
            errors.Add("Last name is required.");

        if (string.IsNullOrWhiteSpace(phoneNumber))
            errors.Add("Phone number is required.");

        if (string.IsNullOrWhiteSpace(address))
            errors.Add("Address is required.");

        if (dateOfBirth == default)
            errors.Add("Date of birth is required.");

        if (dateOfBirth > DateTime.UtcNow.Date)
            errors.Add("Date of birth cannot be in the future.");

        return errors;
    }
}