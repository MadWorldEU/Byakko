namespace MadWorldEU.Byakko.Connections;

/// <summary>Configuration options for the SMTP mail service.</summary>
public sealed class MailOptions
{
    public const string SectionName = "Mail";
    
    /// <summary>Mail provider mode (e.g. <c>Mailpit</c>, <c>Smtp</c>).</summary>
    public string Mode { get; init; } = string.Empty;
    /// <summary>SMTP server hostname.</summary>
    public string Host { get; internal set; } = string.Empty;
    /// <summary>SMTP server port.</summary>
    public int Port { get; internal set; }
    /// <summary>Whether to use SSL/TLS when connecting to the SMTP server.</summary>
    public bool EnableSsl { get; internal set; }
    /// <summary>SMTP authentication username.</summary>
    public string Username { get; init; } = string.Empty;
    /// <summary>SMTP authentication token or password.</summary>
    public string Token { get; init; } = string.Empty;
    /// <summary>Sender address used on outgoing administrator emails.</summary>
    public MailAddressOptions AdministratorFrom { get; init; } = new();
    /// <summary>Recipient address for outgoing administrator emails.</summary>
    public MailAddressOptions AdministratorTo { get; init; } = new();
    
    public bool HasAuthentication => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Token);
}