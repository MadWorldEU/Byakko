using MadWorldEU.Byakko.Connections;
using MailKit.Security;
using Microsoft.Extensions.Logging;

namespace MadWorldEU.Byakko.Correspondences;

public sealed class MailContext(IOptions<MailOptions> options, ILogger<MailService> logger)
{
    private readonly MailOptions _options = options.Value;

    public async Task<bool> CanConnectAsync()
    {
        try
        {
            await WithSmtpClientAsync(_ => Task.CompletedTask);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to connect to SMTP server.");
            return false;
        }
    }

    public async Task<Result> TrySendMessageAsync(MimeMessage mimeMessage)
    {
        try
        {
            await WithSmtpClientAsync(async client =>
            {
                if (_options.HasAuthentication)
                {
                    await client.AuthenticateAsync(_options.Username, _options.Token);
                    logger.LogInformation("Authenticated with SMTP server as {Username}.", _options.Username);
                }

                await client.SendAsync(mimeMessage);
                logger.LogInformation("Email sent successfully to {Recipient}.", mimeMessage.To);
            });

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email via SMTP.");
            return Result.Failure(MailErrors.SendFailed);
        }
    }

    private async Task WithSmtpClientAsync(Func<SmtpClient, Task> action)
    {
        using var client = new SmtpClient();
        client.CheckCertificateRevocation = false;

        var secureSocketOptions = GetSecureSocketOptions();
        await client.ConnectAsync(_options.Host, _options.Port, secureSocketOptions);
        logger.LogInformation("Connected to SMTP server {Host}:{Port}.", _options.Host, _options.Port);

        try
        {
            await action(client);
        }
        finally
        {
            await client.DisconnectAsync(true);
            logger.LogInformation("Disconnected from SMTP server {Host}:{Port}.", _options.Host, _options.Port);
        }
    }

    private SecureSocketOptions GetSecureSocketOptions() =>
        _options.TlsEnabled
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.None;
}