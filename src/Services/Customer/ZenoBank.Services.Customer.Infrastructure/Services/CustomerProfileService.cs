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

    public CustomerProfileService(ICustomerProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CustomerProfileDto>> CreateAsync(Guid userId, CreateCustomerProfileRequest request, CancellationToken cancellationToken = default)
    {
        var errors = ValidateCreateOrUpdate(request.FirstName, request.LastName, request.PhoneNumber, request.Address, request.DateOfBirth);

        if (errors.Count > 0)
            return Result<CustomerProfileDto>.Failure("Validation failed.", errors);

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
            Status = CustomerStatus.Active
        };

        await _repository.AddAsync(profile, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

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
        var errors = ValidateCreateOrUpdate(request.FirstName, request.LastName, request.PhoneNumber, request.Address, request.DateOfBirth);

        if (errors.Count > 0)
            return Result<CustomerProfileDto>.Failure("Validation failed.", errors);

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

        return Result<CustomerProfileDto>.Success(Map(profile), "Customer profile updated successfully.");
    }

    public async Task<Result<List<CustomerProfileDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var profiles = await _repository.GetAllAsync(cancellationToken);
        var data = profiles.Select(Map).ToList();

        return Result<List<CustomerProfileDto>>.Success(data, "Customer profiles fetched successfully.");
    }

    public async Task<Result<CustomerProfileDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var profile = await _repository.GetByIdAsync(id, cancellationToken);
        if (profile is null)
            return Result<CustomerProfileDto>.Failure("Customer profile not found.");

        return Result<CustomerProfileDto>.Success(Map(profile), "Customer profile fetched successfully.");
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
            Status = profile.Status.ToString()
        };
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
