namespace ZenoBank.Services.Notification.Application.Abstractions.Services;

public interface IEmailSender
{
    Task SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken cancellationToken = default);
}