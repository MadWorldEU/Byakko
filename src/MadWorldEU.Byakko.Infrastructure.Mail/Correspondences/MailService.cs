using MadWorldEU.Byakko.Connections;
using MailKit.Security;
using Microsoft.Extensions.Logging;

namespace MadWorldEU.Byakko.Correspondences;

/// <summary>SMTP-based implementation of <see cref="ICorrespondenceService"/> for sending mail.</summary>
public sealed class MailService(MailContext mailContext, IOptions<MailOptions> options, ILogger<MailService> logger) : ICorrespondenceService
{
    private readonly MailOptions _options = options.Value;

    /// <summary>Sends a plain-text email to the configured administrator address.</summary>
    public async Task<Result> SendToAdministratorAsync(string title, string message)
    {
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(_options.AdministratorFrom.Name, _options.AdministratorFrom.Address));
        mimeMessage.To.Add(new MailboxAddress(_options.AdministratorTo.Name, _options.AdministratorTo.Address));
        mimeMessage.Subject = title;

        mimeMessage.Body = new TextPart("plain")
        {
            Text = message
        };

        logger.LogInformation("Mail message built with subject '{Subject}' for {Recipient}.", title, mimeMessage.To);
        return await mailContext.TrySendMessageAsync(mimeMessage);
    }
}