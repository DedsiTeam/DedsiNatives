using DedsiNative.Interfaces;
using Microsoft.Extensions.Logging;

namespace DedsiNative.Services;

public class LoggerEmailSender(ILogger<LoggerEmailSender> logger): IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string message)
    {
        logger.LogInformation("to: {email}, subject: {emailSubject}, message: {emailMessage}", email, subject, message);
        return Task.CompletedTask;
    }
}