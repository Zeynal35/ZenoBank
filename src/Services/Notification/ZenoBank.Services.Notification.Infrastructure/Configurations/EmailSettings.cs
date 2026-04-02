namespace ZenoBank.Services.Notification.Infrastructure.Configurations;

public class EmailSettings
{
    public const string SectionName = "Email";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "ZenoBank";
    public bool UseSsl { get; set; } = true;
}
