namespace ZenoBank.Services.Customer.Application.DTOs;

public class CustomerProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}