namespace MadWorldEU.Byakko.Connections;

/// <summary>Represents a named email address used as a sender or recipient.</summary>
public sealed class MailAddressOptions
{
    /// <summary>Display name shown in the email client.</summary>
    public string Name { get; init; } = string.Empty;
    /// <summary>Email address.</summary>
    public string Address { get; init; } = string.Empty;
}