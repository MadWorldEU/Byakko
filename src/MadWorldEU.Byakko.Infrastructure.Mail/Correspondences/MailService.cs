using MadWorldEU.Byakko.Connections;
using MailKit.Security;
using Microsoft.Extensions.Logging;

namespace MadWorldEU.Byakko.Correspondences;

/// <summary>SMTP-based implementation of <see cref="ICorrespondenceService"/> for sending mail.</summary>
public sealed class MailService(IOptions<MailOptions> options, ILogger<MailService> logger) : ICorrespondenceService
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
        return await TrySendMessageAsync(mimeMessage);
    }

    private async Task<Result> TrySendMessageAsync(MimeMessage mimeMessage)
    {
        try
        {
            using var client = new SmtpClient();
            client.CheckCertificateRevocation = false;

            var secureSocketOptions = GetSecureSocketOptions();
            await client.ConnectAsync(_options.Host, _options.Port, secureSocketOptions);
            logger.LogInformation("Connected to SMTP server {Host}:{Port}.", _options.Host, _options.Port);

            if (_options.HasAuthentication)
            {
                await client.AuthenticateAsync(_options.Username, _options.Token);
                logger.LogInformation("Authenticated with SMTP server as {Username}.", _options.Username);
            }

            await client.SendAsync(mimeMessage);
            logger.LogInformation("Email sent successfully to {Recipient}.", mimeMessage.To);
            
            await client.DisconnectAsync(true);
            logger.LogInformation("Disconnected from SMTP server {Host}:{Port}.", _options.Host, _options.Port);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email via SMTP.");
            return Result.Failure(MailErrors.SendFailed);
        }
    }

    private SecureSocketOptions GetSecureSocketOptions() => 
        _options.TlsEnabled 
            ? SecureSocketOptions.StartTls 
            : SecureSocketOptions.None;
}