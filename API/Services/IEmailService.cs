namespace API.Services;

public interface IEmailService
{
    Task SendEmailAsync(string templateName, EmailData emailData, dynamic content);
}

public record EmailData(string To, string Subject);
