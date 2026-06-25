namespace MadWorldEU.Byakko.Correspondences;

/// <summary>SMTP-based implementation of <see cref="ICorrespondenceService"/> for sending mail.</summary>
public sealed class MailService : ICorrespondenceService
{
    /// <summary>Sends a plain-text email to the configured administrator address.</summary>
    public Result SendToAdministrator(string title, string message)
    {
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress("Joey Tribbiani", "joey@friends.com"));
        mimeMessage.To.Add(new MailboxAddress("Mrs. Chanandler Bong", "chandler@friends.com"));
        mimeMessage.Subject = title;

        mimeMessage.Body = new TextPart("plain")
        {
            Text = message
        };

        using (var client = new SmtpClient())
        {
            client.Connect("smtp.friends.com", 587, false);

            // Note: only needed if the SMTP server requires authentication
            client.Authenticate("joey", "password");

            client.Send(mimeMessage);
            client.Disconnect(true);
        }

        return Result.Success();
    }
}