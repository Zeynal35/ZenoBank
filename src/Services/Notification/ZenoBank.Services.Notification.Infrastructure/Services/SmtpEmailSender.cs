using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using ZenoBank.Services.Notification.Application.Abstractions.Services;
using ZenoBank.Services.Notification.Infrastructure.Configurations;

namespace ZenoBank.Services.Notification.Infrastructure.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;

    public SmtpEmailSender(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;

        message.Body = new BodyBuilder
        {
            HtmlBody = htmlBody
        }.ToMessageBody();

        using var client = new SmtpClient();

        var secureSocketOptions = _settings.UseSsl
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.Auto;

        await client.ConnectAsync(_settings.Host, _settings.Port, secureSocketOptions, cancellationToken);

        if (!string.IsNullOrWhiteSpace(_settings.UserName))
        {
            await client.AuthenticateAsync(_settings.UserName, _settings.Password, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
