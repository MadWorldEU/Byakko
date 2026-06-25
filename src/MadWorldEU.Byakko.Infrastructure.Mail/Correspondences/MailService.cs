using MadWorldEU.Byakko.Connections;
using Microsoft.Extensions.Options;

namespace MadWorldEU.Byakko.Correspondences;

/// <summary>SMTP-based implementation of <see cref="ICorrespondenceService"/> for sending mail.</summary>
public sealed class MailService(IOptions<MailOptions> options) : ICorrespondenceService
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

        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(_options.Host, _options.Port, _options.EnableSsl);

            if (_options.HasAuthentication)
            {
                await client.AuthenticateAsync(_options.Username, _options.Token);
            }

            await client.SendAsync(mimeMessage);
            await client.DisconnectAsync(true);
        }

        return Result.Success();
    }
}