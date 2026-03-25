namespace ZenoBank.Services.Identity.Infrastructure.Configurations;

public class SeedUserSettings
{
    public const string SectionName = "SeedUsers";

    public SeedUserItem SuperAdmin { get; set; } = new();
    public SeedUserItem Admin { get; set; } = new();
    public SeedUserItem Operator { get; set; } = new();
}

public class SeedUserItem
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
