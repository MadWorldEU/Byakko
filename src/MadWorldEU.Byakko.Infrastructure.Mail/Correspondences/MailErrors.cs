namespace MadWorldEU.Byakko.Correspondences;

/// <summary>Errors raised by the mail infrastructure.</summary>
internal static class MailErrors
{
    internal static readonly Error SendFailed = Error.Create("Mail.SendFailed", "Failed to send the email.");
}