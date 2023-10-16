using FluentEmail.Core;
using HandlebarsDotNet;

namespace API.Services;

public class LoggingEmailService : IEmailService
{
    private readonly ILogger<LoggingEmailService> logger;

    public LoggingEmailService(ILogger<LoggingEmailService> logger)
    {
        this.logger = logger;
    }

    public Task SendEmailAsync(string templateName, EmailData emailData, dynamic content)
    {
        var templatePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Templates",
            "Emails",
            $"{templateName}Template.hbs");

        var template = File.ReadAllText(templatePath);

        var compiledTemplate = Handlebars.Compile(template);

        var renderedTemplate = compiledTemplate(content);

        IFluentEmail email = Email
            .From("")
            .To(emailData.To)
            .Subject(emailData.Subject)
            .Body(renderedTemplate, isHtml: true);

        logger.LogInformation(email.Data.Body);

        return Task.CompletedTask;
    }
}
